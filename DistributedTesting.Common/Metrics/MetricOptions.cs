﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedTesting.Common.Metrics
{
    public class MetricsOptions
    {
        public bool Enabled { get; set; }

        public bool InfluxEnabled { get; set; }

        public bool PrometheusEnabled { get; set; }

        public string PrometheusFormatter { get; set; }

        public string InfluxUrl { get; set; }

        public string Database { get; set; }

        public int Interval { get; set; }

        public IDictionary<string, string> Tags { get; set; }
    }
}
