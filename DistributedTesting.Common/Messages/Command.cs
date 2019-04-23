using DistributedTesting.Common.RabbitMq;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedTesting.Common.Messages
{
    public class Command<T> : Message<T>, IRequest
    {
    }
}
