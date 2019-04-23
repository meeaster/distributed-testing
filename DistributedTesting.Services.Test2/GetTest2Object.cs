using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedTesting.Services.Test2
{
    public class GetTest2Object : IRequest<Test2Object>
    {
        public Guid Id { get; set; }
    }
}
