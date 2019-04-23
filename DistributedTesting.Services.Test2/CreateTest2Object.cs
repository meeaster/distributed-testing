using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedTesting.Services.Test2
{
    public class CreateTest2Object : IRequest
    {
        public string String2 { get; set; }

        public int Int2 { get; set; }
    }
}
