using Blueprint.Authorisation;
using Blueprint.Http;

namespace Blueprint.Sample.WebApi.Api
{
    [RootLink("source")]
    [AllowAnonymous]
    public class SourceCodeQuery : IQuery<PlainTextResult>
    {
        public PlainTextResult Invoke(IApiOperationExecutor executor)
        {
            var codeDidIGenerate = ((CodeGennedExecutor)executor).WhatCodeDidIGenerate()
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");

            var template = $@"
<!DOCTYPE html>
<html>
<body>
    <style>body {{ margin: 0 }}</style>
	<link href=""https://cdnjs.cloudflare.com/ajax/libs/prism/1.17.1/themes/prism-okaidia.css"" rel=""stylesheet"" />
    <pre style=""margin: 0; border-radius: 0;""><code class=""language-csharp"">{codeDidIGenerate}</code></pre>
    <script src=""https://cdnjs.cloudflare.com/ajax/libs/prism/1.17.1/prism.js""></script>
    <script src=""https://cdnjs.cloudflare.com/ajax/libs/prism/1.17.1/components/prism-csharp.js""></script>
</body>
</html>";

            return new PlainTextResult(template)
            {
                ContentType = "text/html"
            };
        }
    }
}
