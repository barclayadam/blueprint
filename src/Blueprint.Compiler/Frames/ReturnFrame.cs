using System;
using System.Threading.Tasks;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    public class ReturnFrame : SyncFrame
    {
        private readonly Type _returnType;

        public ReturnFrame()
        {
        }

        public ReturnFrame(Type returnType)
        {
            this._returnType = returnType;
        }

        public ReturnFrame(Variable returnVariable)
        {
            this.ReturnedVariable = returnVariable;
        }

        public Variable ReturnedVariable { get; private set; }

        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            if (this.ReturnedVariable == null && this._returnType != null)
            {
                this.ReturnedVariable = variables.FindVariable(this._returnType);
            }

            if (this.ReturnedVariable == null)
            {
                writer.WriteLine("return;");
            }
            else
            {
                var variableIsTask = this.ReturnedVariable.VariableType.IsGenericType && this.ReturnedVariable.VariableType.GetGenericTypeDefinition() == typeof(Task<>);
                var methodReturnsTask = method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);

                // This method does not use async/await but _does_ return Task, but the variable to return is _not_ a Task<>, therefore we
                // need to use Task.FromResult to get the correct return type
                if (method.AsyncMode == AsyncMode.None && methodReturnsTask && !variableIsTask)
                {
                    // What type are we expecting to return?
                    var taskValueType = method.ReturnType.GenericTypeArguments[0];

                    writer.WriteLine(
                        $"return {typeof(Task).FullNameInCode()}.{nameof(Task.FromResult)}(({taskValueType.FullNameInCode()}){this.ReturnedVariable});");
                }
                else
                {
                    writer.WriteLine($"return {this.ReturnedVariable};");
                }
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.ReturnedVariable == null ? "return;" : $"return {this.ReturnedVariable};";
        }
    }
}
