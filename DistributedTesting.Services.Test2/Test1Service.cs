using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTesting.Services.Test2
{
    public class Test1Service
    {
        private readonly IDistributedCache _distributedCache;

        public Test1Service(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task CreateAsync(Test1Object test1Object, CancellationToken cancellationToken)
        {
            var str = JsonConvert.SerializeObject(test1Object);
            await _distributedCache.SetStringAsync(test1Object.Id, str,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                    SlidingExpiration = TimeSpan.FromMinutes(1)
                }, cancellationToken);

            var test2Object = await _distributedCache.GetStringAsync(test1Object.Id, cancellationToken);
        }
    }
}
