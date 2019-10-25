using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler
{
    public class GeneratedMethod
    {
        private AsyncMode asyncMode = AsyncMode.None;
        private Frame top;

        internal GeneratedMethod(GeneratedType generatedType, MethodInfo method)
        {
            GeneratedType = generatedType;
            ReturnType = method.ReturnType;
            Arguments = method.GetParameters().Select(x => new Argument(x)).ToArray();
            MethodName = method.Name;
            Sources.Add(generatedType);
        }

        internal GeneratedMethod(GeneratedType generatedType, string methodName, Type returnType, params Argument[] arguments)
        {
            GeneratedType = generatedType;
            ReturnType = returnType;
            Arguments = arguments;
            MethodName = methodName;
            Sources.Add(generatedType);
        }

        /// <summary>
        /// Gets the generated type this method belongs to.
        /// </summary>
        public GeneratedType GeneratedType { get; }

        /// <summary>
        /// Gets the return type of the method being generated.
        /// </summary>
        public Type ReturnType { get; }

        /// <summary>
        /// Gets the name of the method being generated.
        /// </summary>
        public string MethodName { get; }

        public bool Overrides { get; set; }

        /// <summary>
        /// Gets or sets the "async mode" of this method (i.e. is the method synchronous, returning a Task, or an async method).
        /// </summary>
        public AsyncMode AsyncMode
        {
            get => asyncMode;
            set => asyncMode = value;
        }

        public Argument[] Arguments { get; }

        public IList<Variable> DerivedVariables { get; } = new List<Variable>();

        public IList<IVariableSource> Sources { get; } = new List<IVariableSource>();

        public Variable ReturnVariable { get; set; }

        public FramesCollection Frames { get; } = new FramesCollection();

        public static GeneratedMethod ForNoArg(GeneratedType type, string name)
        {
            return new GeneratedMethod(type, name, typeof(void), new Argument[0]);
        }

        public static GeneratedMethod ForNoArg<TReturn>(GeneratedType type, string name)
        {
            return new GeneratedMethod(type, name, typeof(TReturn), new Argument[0]);
        }

        public void WriteMethod(ISourceWriter writer)
        {
            if (top == null)
            {
                throw new InvalidOperationException($"You must call {nameof(ArrangeFrames)}() before writing out the source code");
            }

            var returnValue = DetermineReturnExpression();

            if (Overrides)
            {
                returnValue = "override " + returnValue;
            }

            var arguments = Arguments.Select(x => x.Declaration).Join(", ");

            writer.Write($"BLOCK:public {returnValue} {MethodName}({arguments})");

            top.GenerateCode(this, writer);

            WriteReturnStatement(writer);

            writer.FinishBlock();
        }

        public void ArrangeFrames(GeneratedType type)
        {
            if (!Frames.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(Frames), "Cannot be an empty list");
            }

            var compiler = new MethodFrameArranger(this, type);
            compiler.Arrange(out asyncMode, out top);
        }

        public string ToExitStatement()
        {
            return AsyncMode == AsyncMode.AsyncTask
                ? "return;"
                : $"return {typeof(Task).FullName}.{nameof(Task.CompletedTask)};";
        }

        /// <summary>
        /// Add a return frame for the method's return type.
        /// </summary>
        public void Return()
        {
            Frames.Return(ReturnType);
        }

        private void WriteReturnStatement(ISourceWriter writer)
        {
            if (ReturnVariable != null)
            {
                writer.Write($"return {ReturnVariable};");
            }
            else if ((AsyncMode == AsyncMode.ReturnCompletedTask || AsyncMode == AsyncMode.None) && ReturnType == typeof(Task))
            {
                writer.Write("return Task.CompletedTask;");
            }
        }

        private string DetermineReturnExpression()
        {
            return AsyncMode == AsyncMode.AsyncTask
                ? "async " + ReturnType.FullNameInCode()
                : ReturnType.FullNameInCode();
        }
    }
}
