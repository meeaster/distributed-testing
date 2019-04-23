using DistributedTesting.Common.Messages;
using DistributedTesting.Common.Operations;
using DistributedTesting.Common.RabbitMq;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTesting.Services.Test1
{
    public class CreateTest1ObjectHandler : AsyncRequestHandler<Command<CreateTest1Object>>
    {
        private readonly Test1Service _test1Service;
        private readonly IBusPublisher _busPublisher;

        public CreateTest1ObjectHandler(Test1Service test1Service, IBusPublisher busPublisher)
        {
            _test1Service = test1Service;
            _busPublisher = busPublisher;
        }

        protected async override Task Handle(Command<CreateTest1Object> request, CancellationToken cancellationToken)
        {
            var test1Object = new Test1Object
            {
                Id = Guid.NewGuid().ToString("N").Substring(0, 24),
                String1 = request.Value.String1,
                Int1 = request.Value.Int1
            };

            await _test1Service.CreateAsync(test1Object, cancellationToken);
            await _busPublisher.PublishAsync(new Test1Created { Id = test1Object.Id, String1 = test1Object.String1, Int1 = test1Object.Int1 }, request.CorrelationContext);
        }
    }
}
