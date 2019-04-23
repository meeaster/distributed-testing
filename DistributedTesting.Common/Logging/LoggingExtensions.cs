using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
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

                context.Configuration.GetSection("seq").Bind(seqOptions);
                context.Configuration.GetSection("serilog").Bind(serilogOptions);

                if (!Enum.TryParse<LogEventLevel>(serilogOptions.Level, true, out var level))
                {
                    level = LogEventLevel.Information;
                }

                loggerConfiguration.Enrich.FromLogContext()
                    .MinimumLevel.Is(level)
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                    .Enrich.WithProperty("ApplicationName", applicationName);

                Configure(loggerConfiguration, level, seqOptions, serilogOptions);
            });
        }

        private static void Configure(LoggerConfiguration loggerConfiguration, LogEventLevel level, SeqOptions seqOptions, SerilogOptions serilogOptions)
        {
            //if (elkOptions.Enabled)
            //{
            //    loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elkOptions.Url))
            //    {
            //        MinimumLogEventLevel = level,
            //        AutoRegisterTemplate = true,
            //        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
            //        IndexFormat = string.IsNullOrWhiteSpace(elkOptions.IndexFormat)
            //            ? "logstash-{0:yyyy.MM.dd}"
            //            : elkOptions.IndexFormat,
            //        ModifyConnectionSettings = connectionConfiguration =>
            //            elkOptions.BasicAuthEnabled
            //                ? connectionConfiguration.BasicAuthentication(elkOptions.Username, elkOptions.Password)
            //                : connectionConfiguration
            //    });
            //}

            if (seqOptions.Enabled)
            {
                loggerConfiguration.WriteTo.Seq(seqOptions.Url, apiKey: seqOptions.ApiKey);
            }

            if (serilogOptions.ConsoleEnabled)
            {
                loggerConfiguration.WriteTo.Console();
            }
        }
    }
}
