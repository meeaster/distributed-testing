using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedTesting.Common.RabbitMq;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenTracing;

namespace DistributedTesting.Services.Test1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Test1Controller : ControllerBase
    {
        private readonly IBusPublisher _busPublisher;
        private readonly ITracer _tracer;

        public Test1Controller(IBusPublisher busPublisher, ITracer tracer)
        {
            _busPublisher = busPublisher;
            _tracer = tracer;
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateTest1Object testObject1, CancellationToken cancellationToken)
        {
            await _busPublisher.SendAsync(testObject1, GetContext<CreateTest1Object>(Guid.NewGuid(), "test1"));

            return Accepted();
        }

        protected ICorrelationContext GetContext<T>(Guid? resourceId = null, string resource = "")
        {
            if (!string.IsNullOrWhiteSpace(resource))
            {
                resource = $"{resource}/{resourceId}";
            }

            return CorrelationContext.Create<T>(Guid.NewGuid(), Guid.Empty, resourceId ?? Guid.Empty,
               HttpContext.TraceIdentifier, HttpContext.Connection.Id, _tracer.ActiveSpan.Context.ToString(),
               Request.Path.ToString(), "en-us", resource);
        }
    }
}