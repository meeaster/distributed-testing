using DistributedTesting.Common.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenTracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTesting.Services.Test2
{
    public class CreateTest2ObjectHandler : AsyncRequestHandler<CreateTest2Object>
    {
        private readonly Test2Service _test2Service;
        private readonly ITracer _tracer;
        private readonly ILogger _logger;

        public CreateTest2ObjectHandler(Test2Service test2Service, ITracer tracer, ILogger<CreateTest2ObjectHandler> logger)
        {
            _test2Service = test2Service;
            _tracer = tracer;
            _logger = logger;
        }

        protected async override Task Handle(CreateTest2Object request, CancellationToken cancellationToken)
        {
            using (var parentScope = _tracer.BuildSpan("CreateTest2Object").StartActive(finishSpanOnDispose: true))
            {
                var test2Object = new Test2Object
                {
                    Id = Guid.NewGuid(),
                    String2 = request.String2,
                    Int2 = request.Int2
                };

                _logger.LogInformation("Creating test 2 object!");
                await _test2Service.CreateAsync(test2Object, cancellationToken);

                _tracer.ActiveSpan?.Log(new Dictionary<string, object> {
                { "event", "CreateTest2Object" },
                { "test2object", test2Object.Id },
                { "string2", test2Object.String2 },
                { "int2", test2Object.Int2 }
            });
            }
        }
    }
}
