using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTesting.Services.Test2
{
    public class GetTest2ObjectHandler : IRequestHandler<GetTest2Object, Test2Object>
    {
        private readonly Test2Service _test2Service;

        public GetTest2ObjectHandler(Test2Service test2Service)
        {
            _test2Service = test2Service;
        }

        public async Task<Test2Object> Handle(GetTest2Object request, CancellationToken cancellationToken)
        {
            return await _test2Service.GetAsync(request.Id, cancellationToken);
        }
    }
}
