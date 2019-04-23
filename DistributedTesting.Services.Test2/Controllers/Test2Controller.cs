using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DistributedTesting.Services.Test2.Controllers
{
    [Route("api_test2/[controller]")]
    [ApiController]
    public class Test2Controller : ControllerBase
    {
        private readonly IMediator _mediator;

        public Test2Controller(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:length(32)}", Name = "GetTest2Object")]
        public async Task<ActionResult<Test2Object>> Get(string id, CancellationToken cancellationToken)
        {
            var test2Object = await _mediator.Send(new GetTest2Object { Id = Guid.Parse(id) }, cancellationToken);

            if (test2Object == null)
            {
                return NotFound();
            }

            return Ok(test2Object);
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateTest2Object testObject2, CancellationToken cancellationToken)
        {
            await _mediator.Send(testObject2, cancellationToken);

            return Accepted();
        }
    }
}