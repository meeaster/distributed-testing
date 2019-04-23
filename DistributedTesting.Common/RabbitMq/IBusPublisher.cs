using System.Threading.Tasks;
using DistributedTesting.Common.Messages;
using MediatR;

namespace DistributedTesting.Common.RabbitMq
{
    public interface IBusPublisher
    {
        Task SendAsync<TCommand>(TCommand command, ICorrelationContext context)
            where TCommand : IRequest;

        Task PublishAsync<TEvent>(TEvent @event, ICorrelationContext context)
            where TEvent : INotification;
    }
}