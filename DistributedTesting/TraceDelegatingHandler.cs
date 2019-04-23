using OpenTracing;
using OpenTracing.Propagation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTesting
{
    public class TraceDelegatingHandler : DelegatingHandler
    {
        private readonly ITracer _tracer;

        public TraceDelegatingHandler(ITracer tracer)
        {
            _tracer = tracer;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // _tracer.Inject(_tracer.ActiveSpan.Context, BuiltinFormats.HttpHeaders, new RequestBuilderCarrier(request.Headers));

            //do stuff and optionally call the base handler..
            return await base.SendAsync(request, cancellationToken);
        }
    }

    public class RequestBuilderCarrier : ITextMap
    {
        private readonly HttpRequestHeaders _headers;

        public RequestBuilderCarrier(HttpRequestHeaders headers)
        {
            _headers = headers;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value)).GetEnumerator();
        }

        public void Set(string key, string value)
        {
            _headers.Add(key, value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
