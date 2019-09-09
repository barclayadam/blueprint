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
        public static GeneratedMethod For<TReturn>(string name, params Argument[] arguments)
        {
            return new GeneratedMethod(name, typeof(TReturn), arguments);
        }

        public static GeneratedMethod ForNoArg(string name)
        {
            return new GeneratedMethod(name, typeof(void), new Argument[0]);
        }

        public static GeneratedMethod ForNoArg<TReturn>(string name)
        {
            return new GeneratedMethod(name, typeof(TReturn), new Argument[0]);
        }

        private AsyncMode asyncMode = AsyncMode.None;
        private Frame top;

        public GeneratedMethod(MethodInfo method)
        {
            ReturnType = method.ReturnType;
            Arguments = method.GetParameters().Select(x => new Argument(x)).ToArray();
            MethodName = method.Name;
        }

        public GeneratedMethod(string methodName, Type returnType, params Argument[] arguments)
        {
            ReturnType = returnType;
            Arguments = arguments;
            MethodName = methodName;
        }

        /// <summary>
        /// The return type of the method being generated
        /// </summary>
        public Type ReturnType { get; }
        
        /// <summary>
        /// The name of the method being generated
        /// </summary>
        public string MethodName { get; }
        
        
        public bool Overrides { get; set; }

        /// <summary>
        /// Is the method synchronous, returning a Task, or an async method
        /// </summary>
        public AsyncMode AsyncMode
        {
            get => asyncMode;
            set => asyncMode = value;
        }
        
        public Argument[] Arguments { get; }

        
        // TODO -- need a test here. It's used within Jasper, but still
        public IList<Variable> DerivedVariables { get; } = new List<Variable>();
        
        
        public IList<IVariableSource> Sources { get; } = new List<IVariableSource>();

        public Variable ReturnVariable { get; set; }
        

        public void WriteMethod(ISourceWriter writer)
        {
            if (top == null) throw new InvalidOperationException($"You must call {nameof(ArrangeFrames)}() before writing out the source code");
            
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

        protected void WriteReturnStatement(ISourceWriter writer)
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


        protected string DetermineReturnExpression()
        {
            return AsyncMode == AsyncMode.AsyncTask
                ? "async " + ReturnType.FullNameInCode()
                : ReturnType.FullNameInCode();
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
        /// Add a return frame for the method's return type
        /// </summary>
        public void Return()
        {
            Frames.Return(ReturnType);
        }
        
        public FramesCollection Frames { get; } = new FramesCollection();
    }
}
