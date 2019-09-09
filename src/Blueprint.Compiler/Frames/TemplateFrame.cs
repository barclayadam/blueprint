using System;
using System.Collections.Generic;
using System.Linq;

using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    public abstract class TemplateFrame : SyncFrame
    {
        private readonly IList<VariableProxy> proxies = new List<VariableProxy>();
        private string template;

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
    }

    public class VariableProxy
    {
        private readonly Type variableType;
        private readonly string name;
        private readonly string substitution;

        public VariableProxy(int index, Type variableType)
        {
            Index = index;
            this.variableType = variableType;

            substitution = $"~{index}~";
        }

        public VariableProxy(int index, Type variableType, string name)
        {
            Index = index;
            this.variableType = variableType;
            this.name = name;
        }

        public Variable Resolve(IMethodVariables variables)
        {
            Variable = string.IsNullOrEmpty(name)
                ? variables.FindVariable(variableType)
                : variables.FindVariableByName(variableType, name);

            return Variable;
        }
        
        public Variable Variable { get; private set; }
        
        public int Index { get; }

        public override string ToString()
        {
            return substitution;
        }

        public string Substitute(string code)
        {
            return code.Replace(substitution, Variable.Usage);
        }
    }
}
