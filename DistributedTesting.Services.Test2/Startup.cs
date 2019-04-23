using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DistributedTesting.Common;
using DistributedTesting.Common.Jaeger;
using DistributedTesting.Common.Operations;
using DistributedTesting.Common.RabbitMq;
using DistributedTesting.Common.Redis;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DistributedTesting.Services.Test2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddConsul(Configuration);
            services.AddMediatR(Assembly.GetEntryAssembly());
            services.AddRedis(Configuration);
            services.AddJaeger(Configuration);
            services.AddOpenTracing();

            services.AddRabbitMq(Configuration);

            services.AddScoped<Test2Service>();
            services.AddScoped<Test1Service>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.RegisterWithConsul(lifetime);

            app.UseRabbitMq()
                .SubscribeEvent<Test1Created>(@namespace: "test1");

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
