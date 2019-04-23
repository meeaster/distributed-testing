using DistributedTesting.Common.Messages;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedTesting.Common.Operations
{
    [MessageNamespace("test1")]
    public class Test1Created : INotification
    {
        public string Id { get; set; }

        public string String1 { get; set; }

        public int Int1 { get; set; }
    }
}
