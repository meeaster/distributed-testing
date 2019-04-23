using System;
using DistributedTesting.Common.Messages;
using MediatR;

namespace DistributedTesting.Common.RabbitMq
{
    public interface IBusSubscriber
    {
        IBusSubscriber SubscribeCommand<TCommand>(string @namespace = null, string queueName = null,
            Func<TCommand, DistributedTestingException, IRejectedEvent> onError = null)
            where TCommand : IRequest;

        IBusSubscriber SubscribeEvent<TEvent>(string @namespace = null, string queueName = null,
            Func<TEvent, DistributedTestingException, IRejectedEvent> onError = null) 
            where TEvent : INotification;
    }
}
