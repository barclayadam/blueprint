using System;
using System.Threading.Tasks;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    public class ReturnFrame : SyncFrame
    {
        private readonly Type returnType;

        public ReturnFrame()
        {
        }

        public ReturnFrame(Type returnType)
        {
            this.returnType = returnType;
        }

        public ReturnFrame(Variable returnVariable)
        {
            ReturnedVariable = returnVariable;
        }

        public Variable ReturnedVariable { get; private set; }

        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            if (ReturnedVariable == null && returnType != null)
            {
                ReturnedVariable = variables.FindVariable(returnType);
            }

            if (ReturnedVariable == null)
            {
                writer.WriteLine("return;");
            }
            else
            {
                var variableIsTask = ReturnedVariable.VariableType.IsGenericType && ReturnedVariable.VariableType.GetGenericTypeDefinition() == typeof(Task<>);
                var methodReturnsTask = method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);

                // This method does not use async/await but _does_ return Task, but the variable to return is _not_ a Task<>, therefore we
                // need to use Task.FromResult to get the correct return type
                if (method.AsyncMode == AsyncMode.None && methodReturnsTask && !variableIsTask)
                {
                    // What type are we expecting to return?
                    var taskValueType = method.ReturnType.GenericTypeArguments[0];

                    writer.WriteLine(
                        $"return {typeof(Task).FullNameInCode()}.{nameof(Task.FromResult)}(({taskValueType.FullNameInCode()}){ReturnedVariable});");
                }
                else
                {
                    writer.WriteLine($"return {ReturnedVariable};");
                }
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ReturnedVariable == null ? "return;" : $"return {ReturnedVariable};";
        }
    }
}
