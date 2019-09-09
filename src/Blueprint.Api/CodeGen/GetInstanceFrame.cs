using System;
using System.Linq.Expressions;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Api.CodeGen
{
    public abstract class GetInstanceFrame<T> : SyncFrame
    {
        public Variable InstanceVariable { get; protected set; }

        public MethodCall Method(Expression<Action<T>> expression)
        {
            return MethodCall.For(expression);
        }
    }
}
