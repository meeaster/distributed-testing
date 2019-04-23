using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedTesting.Common
{
    public class ConsulConfig
    {
        public string Address { get; set; }

        public string ServiceID { get; set; }

        public string ServiceName { get; set; }
    }
}
