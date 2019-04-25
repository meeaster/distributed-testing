using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CorrelationId;
using DistributedTesting.Common;
using DistributedTesting.Common.Jaeger;
using DistributedTesting.Common.Logging;
using DistributedTesting.Common.Messages;
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

namespace DistributedTesting.Services.Test1
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
            services.AddRabbitMq(Configuration);
            services.AddRedis(Configuration);
            services.AddJaeger(Configuration);
            services.AddOpenTracing();
            //services.AddCorrelationId();

            services.AddTransient(typeof(IRequestHandler<Command<CreateTest1Object>>), typeof(CreateTest1ObjectHandler));

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

            //app.UseLoggingCorrelationId();
            //app.UseCorrelationId(new CorrelationIdOptions
            //{
            //    Header = "X-Correlation-ID",
            //    UseGuidForCorrelationId = true,
            //    UpdateTraceIdentifier = true
            //});

            app.RegisterWithConsul(lifetime);
            app.UseRabbitMq()
                .SubscribeCommand<CreateTest1Object>();

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
