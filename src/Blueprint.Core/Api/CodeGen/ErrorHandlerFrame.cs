using System;
using System.Collections.Generic;
using System.Linq;

using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Core.Api.CodeGen
{
    /// <summary>
    /// A frame that will output try/catch where the catch frames and defined externally on the <see cref="MiddlewareBuilderContext"/>,
    /// defined per exception type to catch (see <see cref="MiddlewareBuilderContext.RegisterUnhandledExceptionHandler" />.
    /// </summary>
    public class ErrorHandlerFrame : SyncFrame
    {
        private readonly MiddlewareBuilderContext context;

        private Variable contextVariable;
        private Dictionary<Type, Frame[]> createdFrames;

        public ErrorHandlerFrame(MiddlewareBuilderContext context)
        {
            this.context = context;
        }

        public override IEnumerable<Variable> Creates
        {
            get
            {
                return GetAllPossibleFrames().SelectMany(x => x.Creates).ToArray();
            }
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write("BLOCK:try");
            Next?.GenerateCode(method, writer);
            writer.FinishBlock();

            foreach (var handler in context.ExceptionHandlers.OrderBy(k => k.Key, new CatchClauseComparer()))
            {
                writer.Write($"BLOCK:catch ({handler.Key.FullNameInCode()} e)");

                foreach (var frame in createdFrames[handler.Key])
                {
                    frame.GenerateCode(method, writer);
                }

                writer.FinishBlock();
            }
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            contextVariable = chain.FindVariable(typeof(ApiOperationContext));

            yield return contextVariable;

            foreach (var f in GetAllPossibleFrames())
            {
                foreach (var v in f.FindVariables(chain))
                {
                    yield return v;
                }
            }
        }

        private IEnumerable<Frame> GetAllPossibleFrames()
        {
            // We must create the frames only once, as otherwise the compiler is not able to handle the assignment of
            // variables etc. as they would be created multiple times.
            if (createdFrames == null)
            {
                createdFrames = new Dictionary<Type, Frame[]>();

                foreach (var handler in context.ExceptionHandlers)
                {
                    createdFrames[handler.Key] = handler.Value(new Variable(handler.Key, "e")).ToArray();
                }
            }
            else
            {
                if (createdFrames.Count != context.ExceptionHandlers.Count)
                {
                    throw new InvalidOperationException("Unhandled exception handler has been modified after code generation.");
                }
            }

            return createdFrames.SelectMany(f => f.Value);
        }

        /// <summary>
        /// A comparer that will order a list of Types (should be <see cref="Exception"/>-derived) such that
        /// the most-specific type is first, as would be expected for a list of catch clauses.
        /// </summary>
        private class CatchClauseComparer : IComparer<Type>
        {
            public int Compare(Type x, Type y)
            {
                if (x == y)
                {
                    return 0;
                }

                if (x.IsAssignableFrom(y))
                {
                    return 1;
                }

                return -1;
            }
        }
    }
}
