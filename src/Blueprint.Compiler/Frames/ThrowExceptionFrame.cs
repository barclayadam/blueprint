using System;
using System.Linq;

namespace Blueprint.Compiler.Frames
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that will throw an exception with a message.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Exception"/> that will be thrown.</typeparam>
    public class ThrowExceptionFrame<T> : SyncFrame
        where T : Exception
    {
        private readonly string exceptionMessage;

        /// <summary>
        /// Initialised a new instance of the <see cref="ThrowExceptionFrame{T}" /> class that will throw
        /// the exception type with the specified message.
        /// </summary>
        /// <param name="exceptionMessage">The exception message to output.</param>
        public ThrowExceptionFrame(string exceptionMessage)
        {
            this.exceptionMessage = exceptionMessage;

            if (!typeof(T).GetConstructors()
                .Any(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType == typeof(string)))
            {
                throw new InvalidOperationException(
                    $"No public constructor that takes a single string parameter could be found for exception type {typeof(T).Name}");
            }
        }

        /// <inheritdoc />
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"throw new {typeof(T).FullNameInCode()}(\"{exceptionMessage.Replace("\"", "\\\"")}\"); ");

            Next?.GenerateCode(method, writer);
        }
    }
}
