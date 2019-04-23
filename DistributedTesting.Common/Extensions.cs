using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributedTesting.Common
{
    public static class Extensions
    {
        public static string Underscore(this string value)
            => string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()));
    }
}
