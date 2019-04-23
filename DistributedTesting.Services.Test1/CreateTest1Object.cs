using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedTesting.Services.Test1
{
    public class CreateTest1Object : IRequest
    {
        public string String1 { get; set; }

        public int Int1 { get; set; }
    }
}
