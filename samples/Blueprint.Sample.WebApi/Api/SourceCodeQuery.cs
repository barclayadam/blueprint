using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Http;

namespace Blueprint.Sample.WebApi.Api
{
    [RootLink("source")]
    public class SourceCodeQuery : IQuery
    {
    }

    public class SourceCodeQueryHandler : IApiOperationHandler<SourceCodeQuery>
    {
        private readonly CodeGennedExecutor executor;

        public SourceCodeQueryHandler(IApiOperationExecutor executor)
        {
            this.executor = (CodeGennedExecutor)executor;
        }

        public Task<object> Invoke(SourceCodeQuery operation, ApiOperationContext apiOperationContext)
        {
            var codeDidIGenerate = executor.WhatCodeDidIGenerate()
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");

            var template = $@"
                <!DOCTYPE html>
                <html>
                <body>
	                <link href=""https://cdnjs.cloudflare.com/ajax/libs/prism/1.17.1/themes/prism.css"" rel=""stylesheet"" />
                    <pre><code class=""language-csharp"">{codeDidIGenerate}</code></pre>
                    <script src=""https://cdnjs.cloudflare.com/ajax/libs/prism/1.17.1/prism.js""></script>
                    <script src=""https://cdnjs.cloudflare.com/ajax/libs/prism/1.17.1/components/prism-csharp.js""></script>
                </body>
                </html>";

            var result = new PlainTextResult(template)
            {
                ContentType = "text/html"
            };

            return Task.FromResult((object)result);
        }
    }
}
