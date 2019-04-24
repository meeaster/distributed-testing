using CorrelationId;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributedTesting.Common.Logging
{
    public static class LoggingExtensions
    {
        public static IWebHostBuilder UseLogging(this IWebHostBuilder webHostBuilder, string applicationName = null)
        {
            return webHostBuilder.UseSerilog((context, loggerConfiguration) =>
            {
                var seqOptions = new SeqOptions();
                var serilogOptions = new SerilogOptions();
                var elkOptions = new ElkOptions();

                context.Configuration.GetSection("seq").Bind(seqOptions);
                context.Configuration.GetSection("serilog").Bind(serilogOptions);
                context.Configuration.GetSection("elk").Bind(elkOptions);

                if (!Enum.TryParse<LogEventLevel>(serilogOptions.Level, true, out var level))
                {
                    level = LogEventLevel.Information;
                }

                loggerConfiguration.Enrich.FromLogContext()
                    .MinimumLevel.Is(level)
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                    .Enrich.WithProperty("ApplicationName", applicationName);

                Configure(loggerConfiguration, level, seqOptions, elkOptions, serilogOptions);
            });
        }

        private static void Configure(LoggerConfiguration loggerConfiguration, LogEventLevel level, SeqOptions seqOptions, ElkOptions elkOptions, SerilogOptions serilogOptions)
        {
            if (elkOptions.Enabled)
            {
                loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elkOptions.Url))
                {
                    MinimumLogEventLevel = level,
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                    IndexFormat = string.IsNullOrWhiteSpace(elkOptions.IndexFormat)
                        ? "logstash-{0:yyyy.MM.dd}"
                        : elkOptions.IndexFormat,
                    ModifyConnectionSettings = connectionConfiguration =>
                        elkOptions.BasicAuthEnabled
                            ? connectionConfiguration.BasicAuthentication(elkOptions.Username, elkOptions.Password)
                            : connectionConfiguration
                });
            }

            if (seqOptions.Enabled)
            {
                loggerConfiguration.WriteTo.Seq(seqOptions.Url, apiKey: seqOptions.ApiKey);
            }

            if (serilogOptions.ConsoleEnabled)
            {
                loggerConfiguration.WriteTo.Console();
            }
        }

        public static void UseLoggingCorrelationId(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.Use(async (context, next) =>
            {
                string correlationId;
                var header = context.Request.Headers.FirstOrDefault(x => x.Key == "X-Correlation-ID").Value;
                if(header.Count != 0)
                {
                    correlationId = header.First();
                }
                else
                {
                    correlationId = Guid.NewGuid().ToString();
                    context.Request.Headers.Add("X-Correlation-ID", new Microsoft.Extensions.Primitives.StringValues(correlationId));
                }

                using (LogContext.PushProperty("X-Correlation-ID", correlationId))
                {
                    await next.Invoke();
                }
            });
        }
    }
}
