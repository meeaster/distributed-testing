using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTesting.Services.Test1
{
    public class Test1Service
    {
        private readonly IMongoCollection<Test1Object> test1Objects;

        public Test1Service(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDb"));
            var database = client.GetDatabase("Test1Db");
            test1Objects = database.GetCollection<Test1Object>("Test1");
        }

        public List<Test1Object> Get()
        {
            return test1Objects.Find(x => true).ToList();
        }

        public Test1Object Get(string id)
        {
            return test1Objects.Find<Test1Object>(x => x.Id == id).FirstOrDefault();
        }

        public async Task CreateAsync(Test1Object test1Object, CancellationToken cancellationToken)
        {
            await test1Objects.InsertOneAsync(test1Object, null, cancellationToken);
        }

        public void Update(string id, Test1Object test1Object)
        {
            test1Objects.ReplaceOne(x => x.Id == id, test1Object);
        }

        public void Remove(Test1Object test1Object)
        {
            test1Objects.DeleteOne(x => x.Id == test1Object.Id);
        }

        public void Remove(string id)
        {
            test1Objects.DeleteOne(x => x.Id == id);
        }
    }
}
