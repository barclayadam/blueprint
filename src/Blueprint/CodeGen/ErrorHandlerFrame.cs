using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.CodeGen
{
    /// <summary>
    /// A frame that will output try/catch where the catch frames and defined externally on the <see cref="MiddlewareBuilderContext"/>,
    /// defined per exception type to catch (see <see cref="MiddlewareBuilderContext.RegisterUnhandledExceptionHandler" />.
    /// </summary>
    public class ErrorHandlerFrame : SyncFrame
    {
        private readonly MiddlewareBuilderContext context;

        /// <summary>
        /// Initialises a new instance of the <see cref="ErrorHandlerFrame" /> class.
        /// </summary>
        /// <param name="context">The builder context for this frame.</param>
        public ErrorHandlerFrame(MiddlewareBuilderContext context)
        {
            this.context = context;
        }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            writer.Block("try");
            next();
            writer.FinishBlock();

            foreach (var handler in context.ExceptionHandlers.OrderBy(k => k.Key, new CatchClauseComparer()))
            {
                writer.Block($"catch ({handler.Key.FullNameInCode()} e)");

                // Key == exception type being caught
                var exceptionVariable = new Variable(handler.Key, "e");
                var allFrames = handler.Value.SelectMany(v => v(exceptionVariable)).ToList();

                foreach (var frame in allFrames)
                {
                    frame.GenerateCode(variables, method, writer);
                }

                writer.FinishBlock();
            }

            if (context.FinallyFrames.Any())
            {
                writer.Block("finally");

                foreach (var frame in context.FinallyFrames)
                {
                    frame.GenerateCode(variables, method, writer);
                }

                writer.FinishBlock();
            }
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
