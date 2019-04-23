using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTesting.Services.Test2
{
    public class Test2Service
    {
        private readonly IDistributedCache _distributedCache;

        public Test2Service(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task CreateAsync(Test2Object test2Object, CancellationToken cancellationToken)
        {
            var str = JsonConvert.SerializeObject(test2Object);
            await _distributedCache.SetStringAsync(test2Object.Id.ToString("N"), str,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                    SlidingExpiration = TimeSpan.FromMinutes(1)
                }, cancellationToken);
        }

        public async Task<Test2Object> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            var test2Object = await _distributedCache.GetStringAsync(id.ToString("N"), cancellationToken);

            return string.IsNullOrWhiteSpace(test2Object) ? null : JsonConvert.DeserializeObject<Test2Object>(test2Object);
        }
    }
}
