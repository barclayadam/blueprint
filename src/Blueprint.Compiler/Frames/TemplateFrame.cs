using System.Collections.Generic;
using System.Linq;

using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    public abstract class TemplateFrame : SyncFrame
    {
        private readonly IList<VariableProxy> proxies = new List<VariableProxy>();
        private string template;

        public sealed override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var code = template;

            foreach (var proxy in proxies)
            {
                code = proxy.Substitute(code);
            }

            writer.Write(code);
            Next?.GenerateCode(method, writer);
        }

        public sealed override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            template = Template();

            return proxies.Select(x => x.Resolve(chain));
        }

        protected abstract string Template();

        protected object Arg<T>()
        {
            var proxy = new VariableProxy(proxies.Count, typeof(T));
            proxies.Add(proxy);

            return proxy;
        }

        protected object Arg<T>(string name)
        {
            var proxy = new VariableProxy(proxies.Count, typeof(T), name);
            proxies.Add(proxy);

            return proxy;
        }
    }
}
