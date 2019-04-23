﻿using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DistributedTesting.Common.Messages;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
// using MongoDB.Driver.Core.Misc;
using OpenTracing;
using OpenTracing.Tag;
using Polly;
using RawRabbit;
using RawRabbit.Common;
using RawRabbit.Enrichers.MessageContext;

namespace DistributedTesting.Common.RabbitMq
{
    public class BusSubscriber : IBusSubscriber
    {
        private readonly ILogger _logger;
        private readonly IBusClient _busClient;
        private readonly IServiceProvider _serviceProvider;        
        private readonly ITracer _tracer;
        private readonly string _defaultNamespace;
        private readonly int _retries;
        private readonly int _retryInterval;

        public BusSubscriber(IApplicationBuilder app)
        {
            _logger = app.ApplicationServices.GetService<ILogger<BusSubscriber>>();
            _serviceProvider = app.ApplicationServices.GetService<IServiceProvider>();
            _busClient = _serviceProvider.GetService<IBusClient>();
            _tracer = _serviceProvider.GetService<ITracer>();
            var options = _serviceProvider.GetService<IOptions<RabbitMqOptions>>();
            _defaultNamespace = options.Value.Namespace;
            _retries = options.Value.Retries >= 0 ? options.Value.Retries : 3;
            _retryInterval = options.Value.RetryInterval > 0 ? options.Value.RetryInterval : 2;
        }

        public IBusSubscriber SubscribeCommand<TCommand>(string @namespace = null, string queueName = null,
            Func<TCommand, DistributedTestingException, IRejectedEvent> onError = null)
            where TCommand : IRequest
        {
            _busClient.SubscribeAsync<TCommand, CorrelationContext>(async (command, correlationContext) =>
                {
                    using(var scope = _serviceProvider.CreateScope())
                    {
                        var mediator = scope.ServiceProvider.GetService<IMediator>();
                        var wrappedCommand = new Command<TCommand>
                        {
                            CorrelationContext = correlationContext,
                            Value = command
                        };

                        return await TryHandleAsync(command, correlationContext, () => mediator.Send(wrappedCommand, CancellationToken.None), onError);
                    }
                },
                ctx => ctx.UseSubscribeConfiguration(cfg =>
                    cfg.FromDeclaredQueue(q => q.WithName(GetQueueName<TCommand>(@namespace, queueName)))));

            return this;
        }

        public IBusSubscriber SubscribeEvent<TEvent>(string @namespace = null, string queueName = null,
            Func<TEvent, DistributedTestingException, IRejectedEvent> onError = null)
            where TEvent : INotification
        {
            _busClient.SubscribeAsync<TEvent, CorrelationContext>(async (@event, correlationContext) =>
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var mediator = scope.ServiceProvider.GetService<IMediator>();
                        var wrappedEvent = new Event<TEvent>
                        {
                            CorrelationContext = correlationContext,
                            Value = @event
                        };

                        return await TryHandleAsync(@event, correlationContext, () => mediator.Publish(wrappedEvent, CancellationToken.None), onError);
                    }
                },
                ctx => ctx.UseSubscribeConfiguration(cfg =>
                    cfg.FromDeclaredQueue(q => q.WithName(GetQueueName<TEvent>(@namespace, queueName)))));

            return this;
        }

        // Internal retry for services that subscribe to the multiple events of the same types.
        // It does not interfere with the routing keys and wildcards (see TryHandleWithRequeuingAsync() below).
        private async Task<Acknowledgement> TryHandleAsync<TMessage>(TMessage message,
            CorrelationContext correlationContext,
            Func<Task> handle, Func<TMessage, DistributedTestingException, IRejectedEvent> onError = null)
        {
            var currentRetry = 0;
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_retries, i => TimeSpan.FromSeconds(_retryInterval));
            
            var messageName = message.GetType().Name;

            return await retryPolicy.ExecuteAsync<Acknowledgement>(async () =>
            {
                var scope = _tracer
                    .BuildSpan("executing-handler")
                    .AsChildOf(_tracer.ActiveSpan)
                    .StartActive(true);

                using (scope)
                {
                    var span = scope.Span;
                    
                    try
                    {
                        var retryMessage = currentRetry == 0
                            ? string.Empty
                            : $"Retry: {currentRetry}'.";

                        var preLogMessage = $"Handling a message: '{messageName}' " +
                                      $"with correlation id: '{correlationContext.Id}'. {retryMessage}";
                        
                        _logger.LogInformation(preLogMessage);
                        span.Log(preLogMessage);

                        await handle();

                        var postLogMessage = $"Handled a message: '{messageName}' " +
                                             $"with correlation id: '{correlationContext.Id}'. {retryMessage}";
                        _logger.LogInformation(postLogMessage);
                        span.Log(postLogMessage);

                        return new Ack();
                    }
                    catch (Exception exception)
                    {
                        currentRetry++;
                        _logger.LogError(exception, exception.Message);
                        span.Log(exception.Message);
                        span.SetTag(Tags.Error, true);
                        
                        if (exception is DistributedTestingException dShopException && onError != null)
                        {
                            var rejectedEvent = onError(message, dShopException);
                            await _busClient.PublishAsync(rejectedEvent, ctx => ctx.UseMessageContext(correlationContext));
                            _logger.LogInformation($"Published a rejected event: '{rejectedEvent.GetType().Name}' " +
                                                   $"for the message: '{messageName}' with correlation id: '{correlationContext.Id}'.");

                            span.SetTag("error-type", "domain");
                            return new Ack();
                        }

                        span.SetTag("error-type", "infrastructure");
                        throw new Exception($"Unable to handle a message: '{messageName}' " +
                                            $"with correlation id: '{correlationContext.Id}', " +
                                            $"retry {currentRetry - 1}/{_retries}...");
                    }
                }
            });
        }
        
        // RabbitMQ retry that will publish a message to the retry queue.
        // Keep in mind that it might get processed by the other services using the same routing key and wildcards.
        private async Task<Acknowledgement> TryHandleWithRequeuingAsync<TMessage>(TMessage message,
            CorrelationContext correlationContext,
            Func<Task> handle, Func<TMessage, DistributedTestingException, IRejectedEvent> onError = null)
        {
            var messageName = message.GetType().Name;
            var retryMessage = correlationContext.Retries == 0
                ? string.Empty
                : $"Retry: {correlationContext.Retries}'.";
            _logger.LogInformation($"Handling a message: '{messageName}' " +
                                   $"with correlation id: '{correlationContext.Id}'. {retryMessage}");

            try
            {
                await handle();
                _logger.LogInformation($"Handled a message: '{messageName}' " +
                                       $"with correlation id: '{correlationContext.Id}'. {retryMessage}");

                return new Ack();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                if (exception is DistributedTestingException dShopException && onError != null)
                {
                    var rejectedEvent = onError(message, dShopException);
                    await _busClient.PublishAsync(rejectedEvent, ctx => ctx.UseMessageContext(correlationContext));
                    _logger.LogInformation($"Published a rejected event: '{rejectedEvent.GetType().Name}' " +
                                           $"for the message: '{messageName}' with correlation id: '{correlationContext.Id}'.");

                    return new Ack();
                }

                if (correlationContext.Retries >= _retries)
                {
                    await _busClient.PublishAsync(RejectedEvent.For(messageName),
                        ctx => ctx.UseMessageContext(correlationContext));

                    throw new Exception($"Unable to handle a message: '{messageName}' " +
                                        $"with correlation id: '{correlationContext.Id}' " +
                                        $"after {correlationContext.Retries} retries.", exception);
                }

                _logger.LogInformation($"Unable to handle a message: '{messageName}' " +
                                       $"with correlation id: '{correlationContext.Id}', " +
                                       $"retry {correlationContext.Retries}/{_retries}...");

                return Retry.In(TimeSpan.FromSeconds(_retryInterval));
            }
        }

        private string GetQueueName<T>(string @namespace = null, string name = null)
        {
            @namespace = string.IsNullOrWhiteSpace(@namespace)
                ? (string.IsNullOrWhiteSpace(_defaultNamespace) ? string.Empty : _defaultNamespace)
                : @namespace;

            var separatedNamespace = string.IsNullOrWhiteSpace(@namespace) ? string.Empty : $"{@namespace}.";

            return (string.IsNullOrWhiteSpace(name)
                ? $"{Assembly.GetEntryAssembly().GetName().Name}/{separatedNamespace}{typeof(T).Name.Underscore()}"
                : $"{name}/{separatedNamespace}{typeof(T).Name.Underscore()}").ToLowerInvariant();
        }
    }
}