using DistributedTesting.Common.RabbitMq;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedTesting.Common.Messages
{
    public class Message<T>
    {
        public CorrelationContext CorrelationContext { get; set; }

        public T Value { get; set; }
    }
}
