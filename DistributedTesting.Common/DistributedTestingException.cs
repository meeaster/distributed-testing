using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedTesting.Common
{
    public class DistributedTestingException : Exception
    {
        public string Code { get; }

        public DistributedTestingException()
        {
        }

        public DistributedTestingException(string code)
        {
            Code = code;
        }

        public DistributedTestingException(string message, params object[] args)
            : this(string.Empty, message, args)
        {
        }

        public DistributedTestingException(string code, string message, params object[] args)
            : this(null, code, message, args)
        {
        }

        public DistributedTestingException(Exception innerException, string message, params object[] args)
            : this(innerException, string.Empty, message, args)
        {
        }

        public DistributedTestingException(Exception innerException, string code, string message, params object[] args)
            : base(string.Format(message, args), innerException)
        {
            Code = code;
        }
    }
}
