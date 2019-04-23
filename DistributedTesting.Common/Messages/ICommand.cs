using MediatR;

namespace DistributedTesting.Common.Messages
{
    //Marker
    public interface ICommand : IRequest, IMessage
    {
    }
}