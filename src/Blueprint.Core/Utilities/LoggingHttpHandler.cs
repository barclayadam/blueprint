using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using NLog;

namespace Blueprint.Core.Utilities
{
    public class LoggingHttpHandler : DelegatingHandler
    {
        private readonly Logger logger;

        public LoggingHttpHandler(Logger logger, HttpClientHandler httpClientHandler)
                : base(httpClientHandler)
        {
            this.logger = logger;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            logger.Info("Executing HTTP request. url={0}", request.RequestUri);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
