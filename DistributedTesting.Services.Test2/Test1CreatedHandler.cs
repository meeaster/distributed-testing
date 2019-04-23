using DistributedTesting.Common.Messages;
using DistributedTesting.Common.Operations;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTesting.Services.Test2
{
    public class Test1CreatedHandler : INotificationHandler<Event<Test1Created>>
    {
        private readonly Test1Service _test1Service;

        public Test1CreatedHandler(Test1Service test1Service)
        {
            _test1Service = test1Service;
        }

        public async Task Handle(Event<Test1Created> notification, CancellationToken cancellationToken)
        {
            var test1Object = new Test1Object
            {
                Id = notification.Value.Id,
                Int1 = notification.Value.Int1,
                String1 = notification.Value.String1
            };

            await _test1Service.CreateAsync(test1Object, cancellationToken);
        }
    }
}
