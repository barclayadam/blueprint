using System;
using System.Linq;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Frames
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that will throw an exception with a message.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Exception"/> that will be thrown.</typeparam>
    public class ThrowExceptionFrame<T> : SyncFrame
        where T : Exception
    {
        private readonly string _exceptionMessage;

        /// <summary>
        /// Initialised a new instance of the <see cref="ThrowExceptionFrame{T}" /> class that will throw
        /// the exception type with the specified message.
        /// </summary>
        /// <param name="exceptionMessage">The exception message to output.</param>
        public ThrowExceptionFrame(string exceptionMessage)
        {
            this._exceptionMessage = exceptionMessage;

            if (!typeof(T).GetConstructors()
                .Any(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType == typeof(string)))
            {
                throw new InvalidOperationException(
                    $"No public constructor that takes a single string parameter could be found for exception type {typeof(T).Name}");
            }
        }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            writer.WriteLine(this.ToString());

            next();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"throw new {typeof(T).FullNameInCode()}(\"{this._exceptionMessage.Replace("\"", "\\\"")}\"); ";
        }
    }
}
