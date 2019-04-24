using CorrelationId;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedTesting.Common.Logging
{
    public class CorrelationIdEnricher : ILogEventEnricher
    {
        private readonly ICorrelationContextAccessor _correlationContextAccessor;

        public CorrelationIdEnricher(ICorrelationContextAccessor correlationContextAccessor)
        {
            _correlationContextAccessor = correlationContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddOrUpdateProperty(new LogEventProperty("CorrelationId", new ScalarValue(_correlationContextAccessor.CorrelationContext.CorrelationId)));
        }
    }
}
