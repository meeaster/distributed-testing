using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;
using RawRabbit.Instantiation;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DistributedTesting.Common.Jaeger
{
    public static class JaegerExtensions
    {
        private static bool _initialized;

        public static IServiceCollection AddJaeger(this IServiceCollection services, IConfiguration configuration)
        {
            if (_initialized)
            {
                return services;
            }

            _initialized = true;
            var section = configuration.GetSection("jaeger");
            services.Configure<JaegerOptions>(section);

            var options = new JaegerOptions();
            section.Bind(options);

            if (!options.Enabled)
            {
                var defaultTracer = new Tracer.Builder(Assembly.GetEntryAssembly().FullName)
                    .WithReporter(new NoopReporter())
                    .WithSampler(new ConstSampler(false))
                    .Build();

                services.AddSingleton(defaultTracer);
                return services;
            }

            services.AddSingleton<ITracer>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                var reporter = new RemoteReporter
                        .Builder()
                    .WithSender(new UdpSender(options.UdpHost, options.UdpPort, options.MaxPacketSize))
                    .WithLoggerFactory(loggerFactory)
                    .Build();

                var sampler = GetSampler(options);

                var tracer = new Tracer
                        .Builder(options.ServiceName)
                    .WithReporter(reporter)
                    .WithSampler(sampler)
                    .Build();

                GlobalTracer.Register(tracer);

                return tracer;
            });

            return services;
        }

        public static IClientBuilder UseJaeger(this IClientBuilder builder, ITracer tracer)
        {
            builder.Register(pipe => pipe
                .Use<JaegerStagedMiddleware>(tracer));

            return builder;
        }

        private static ISampler GetSampler(JaegerOptions options)
        {
            switch (options.Sampler)
            {
                case "const":
                    return new ConstSampler(true);
                case "rate":
                    return new RateLimitingSampler(options.MaxTracesPerSecond);
                case "probabilistic":
                    return new ProbabilisticSampler(options.SamplingRate);
                default:
                    return new ConstSampler(true);
            }
        }
    }
}
