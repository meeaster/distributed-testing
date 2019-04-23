using MediatR;

namespace DistributedTesting.Common.Messages
{
    //Marker
    public interface IEvent : INotification, IMessage
    {
    }
}