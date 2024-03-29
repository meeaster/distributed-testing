﻿using DistributedTesting.Common.Messages;
using DistributedTesting.Common.RabbitMq;
using Jaeger;
using OpenTracing;
using OpenTracing.Tag;
using RawRabbit.Pipe;
using RawRabbit.Pipe.Middleware;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTesting.Common.Jaeger
{
    public class JaegerStagedMiddleware : StagedMiddleware
    {
        private readonly ITracer _tracer;

        public JaegerStagedMiddleware(ITracer tracer)
            => _tracer = tracer;

        public override string StageMarker => RawRabbit.Pipe.StageMarker.MessageDeserialized;


        public override Task InvokeAsync(IPipeContext context, CancellationToken token = new CancellationToken())
        {
            var correlationContext = (ICorrelationContext)context.GetMessageContext();
            var messageType = context.GetMessageType();

            using (var scope = BuildScope(messageType, correlationContext.SpanContext))
            {
                var span = scope.Span;
                span.Log($"Processing {messageType.Name}");

                try
                {
                    return Next.InvokeAsync(context, token);
                }
                catch (Exception ex)
                {
                    span.SetTag(Tags.Error, true);
                    span.Log(ex.Message);
                }
            }

            return Next.Next.InvokeAsync(context, token);
        }

        private IScope BuildScope(Type messageType, string serializedSpanContext)
        {
            var spanBuilder = _tracer
                .BuildSpan($"processing-{messageType.Name}")
                .WithTag("message-type", $@"{(typeof(ICommand).IsAssignableFrom(messageType) ? "command" : "event")}");

            if (string.IsNullOrEmpty(serializedSpanContext))
            {
                return spanBuilder.StartActive(true);
            }

            var spanContext = SpanContext.ContextFromString(serializedSpanContext);

            return spanBuilder
                .AddReference(References.FollowsFrom, spanContext)
                .StartActive(true);
        }
    }
}
