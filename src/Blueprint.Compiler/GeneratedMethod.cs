using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler;

/// <summary>
/// A method, declared on a <see cref="GeneratedType" />.
/// </summary>
public class GeneratedMethod : IMethodVariables
{
    // The variables that are used / created within the body of this GeneratedMethod. Keeps track of
    // them to ensure the same variable frame is not injected twice (i.e. if multiple frames asks for a variable
    // of a given type it should resolve to that same variable instance when it can)
    private readonly Dictionary<Type, Variable> _variables = new Dictionary<Type, Variable>();

    // A list of every frame that has contributed to the code of this method. This is built up during
    // code generation (see WriteMethod) by each Frame calling RegisterFrame
    private readonly List<Frame> _allRegisteredFrames = new List<Frame>();

    internal GeneratedMethod(GeneratedType generatedType, MethodInfo method)
    {
        this.GeneratedType = generatedType;
        this.ReturnType = method.ReturnType;
        this.Arguments = method.GetParameters().Select(x => new Argument(x)).ToArray();
        this.MethodName = method.Name;
        this.Sources.Add(generatedType);
    }

    internal GeneratedMethod(GeneratedType generatedType, string methodName, Type returnType, params Argument[] arguments)
    {
        this.GeneratedType = generatedType;
        this.ReturnType = returnType;
        this.Arguments = arguments;
        this.MethodName = methodName;
        this.Sources.Add(generatedType);
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

    /// <summary>
    /// Gets or sets a value indicating whether this method <c>overrides</c> a method from it's
    /// base class.
    /// </summary>
    public bool Overrides { get; set; }

    /// <summary>
    /// Gets the "async mode" of this method (i.e. is the method synchronous, returning a Task, or an async method).
    /// </summary>
    public AsyncMode AsyncMode { get; internal set; } = AsyncMode.None;

    /// <summary>
    /// Gets the <see cref="Argument"/>s of this method, which may be empty.
    /// </summary>
    public Argument[] Arguments { get; }

    /// <summary>
    /// Gets the list of <see cref="IVariableSource"/>s that can be used to find
    /// variables that are not created by a <see cref="Frame" /> that has been
    /// directly added to this method (i.e. a source could create frames on the fly
    /// to fulfil a variable request).
    /// </summary>
    public IList<IVariableSource> Sources { get; } = new List<IVariableSource>();

    /// <summary>
    /// Gets the collection of <see cref="Frame"/>s that make up this method.
    /// </summary>
    /// <remarks>
    /// This collection is the <em>top level</em> frames, some of which may be a
    /// <see cref="CompositeFrame" /> that itself contains a set of frames.
    /// </remarks>
    public FramesCollection Frames { get; } = new FramesCollection();

    /// <summary>
    /// Creates a new <see cref="GeneratedMethod"/> with <c>void</c> return, no arguments and
    /// the given name and adds it to the given <see cref="GeneratedType" />.
    /// </summary>
    /// <param name="type">The type to add to.</param>
    /// <param name="name">The name of the method.</param>
    /// <returns>A new <see cref="GeneratedMethod"/>.</returns>
    public static GeneratedMethod ForNoArg(GeneratedType type, string name)
    {
        return new GeneratedMethod(type, name, typeof(void), new Argument[0]);
    }

    /// <summary>
    /// Creates a new <see cref="GeneratedMethod"/> with no arguments, <typeparamref name="TReturn"/> return type
    /// and the given name and adds it to the given <see cref="GeneratedType" />.
    /// </summary>
    /// <typeparam name="TReturn">The return type of the method.</typeparam>
    /// <param name="type">The type to add to.</param>
    /// <param name="name">The name of the method.</param>
    /// <returns>A new <see cref="GeneratedMethod"/>.</returns>
    public static GeneratedMethod ForNoArg<TReturn>(GeneratedType type, string name)
    {
        return new GeneratedMethod(type, name, typeof(TReturn), new Argument[0]);
    }

    /// <summary>
    /// Add a return frame for the method's return type.
    /// </summary>
    public void Return()
    {
        this.Frames.Return(this.ReturnType);
    }

    /// <summary>
    /// Writes the code for this method to the specified <see cref="ISourceWriter" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method will perform the necessary work to collect variables, both created in this method
    /// and by external <see cref="IVariableSource"/>s.
    /// </para>
    /// <para>
    /// This method does a lot of bookkeeping to keep track of <see cref="Frame"/>s and <see cref="Variable"/>s that
    /// are created and used by any added frames. The "top" <see cref="Frame"/> will have it's <see cref="Frame.Generate"/>
    /// method called twice, which in turn is expected to flow through all frames (using the assigned <see cref="Frame.NextFrame" />
    /// property.
    /// </para>
    /// <para>
    /// The <see cref="Frame.Generate" /> method is called twice to allow the first run-through to create the required variables
    /// and to declare the dependencies. During this process we collect this information and, potentially, add in extra <see cref="Frame"/>s
    /// as required by the <see cref="IVariableSource"/>s that declare on-the-fly variables. The second time we will already have
    /// determined the variables and will actually write the code to the supplied <see cref="ISourceWriter" />.
    /// </para>
    /// </remarks>
    /// <param name="writer">The writer to output the code to.</param>
    public void WriteMethod(ISourceWriter writer)
    {
        // 1. Chain all existing frames together (setting their NextFrame property).
        var topFrame = ChainFrames(this.Frames);

        // 2. Clear out "all registered frames" to enable this method to be called multiple times.
        this._allRegisteredFrames.Clear();

        // 3. The first time around is used for discovering the variables, ensuring frames
        // are fully created etc. No actual writing will occur
        var trackingWriter = new TrackingVariableWriter(this);
        topFrame.GenerateCode(trackingWriter, this, new MethodSourceWriter(trackingWriter, this, trackingWriter));

        // 4. Determine the async mode of this method, which determines the result type and how
        // the actual return value is generated. Only do this is asyncMode is not set to
        // something else to allow overriding externally
        if (this.AsyncMode == AsyncMode.None)
        {
            this.AsyncMode = AsyncMode.AsyncTask;

            if (this._allRegisteredFrames.All(x => !x.IsAsync))
            {
                this.AsyncMode = AsyncMode.None;
            }
            else
            {
                var lastFrame = this._allRegisteredFrames.Last();

                if (this._allRegisteredFrames.Count(x => x.IsAsync) == 1 && lastFrame.IsAsync && lastFrame.CanReturnTask())
                {
                    this.AsyncMode = AsyncMode.ReturnFromLastNode;
                }
            }
        }

        // 5. Now find various types of variables to push to the GeneratedType, in addition to also
        // adding the creation frames to this method's collection if they do not already exist
        foreach (var variable in this._variables.Values.TopologicalSort(v => v.Dependencies))
        {
            if (variable.Creator != null && !this._allRegisteredFrames.Contains(variable.Creator))
            {
                // Find the first usage of this variable and place the frame before that.
                var firstUsage = this._allRegisteredFrames.FirstOrDefault(f => f.Uses.Contains(variable));

                if (firstUsage == null)
                {
                    this.Frames.Insert(0, variable.Creator);

                    // throw new InvalidOperationException(
                    //     $"The variable '{variable}' has a creator Frame that has not been appended to this GeneratedMethod, " +
                    //     "nor does any Frame exist that Uses it, therefore we cannot determine where to place the creator Frame.");
                }
                else
                {
                    var indexOf = this.Frames.IndexOf(firstUsage);

                    // TODO: This is a little nasty, dumping the creator frame at the start of the method. It _should_ work in many cases
                    // but is far from ideal
                    this.Frames.Insert(indexOf == -1 ? 0 : indexOf, variable.Creator);
                }

                this._allRegisteredFrames.Add(variable.Creator);
            }

            switch (variable)
            {
                case InjectedField field:
                    this.GeneratedType.AllInjectedFields.Add(field);
                    break;

                case Property setter:
                    this.GeneratedType.Properties.Add(setter);
                    break;

                case StaticField staticField:
                    this.GeneratedType.AllStaticFields.Add(staticField);
                    break;
            }
        }

        // 6. Re-chain all existing frames as we may have pushed new ones
        topFrame = ChainFrames(this.Frames);

        // 7. We now have all frames & variables collected, lets do the final generation of code
        var returnValue = this.DetermineReturnExpression();

        if (this.Overrides)
        {
            returnValue = "override " + returnValue;
        }

        var tempQualifier = this.Arguments.Select(x => x.Declaration);
        var arguments = string.Join(", ", tempQualifier);

        writer.Block($"public {returnValue} {this.MethodName}({arguments})");

        // 7.1. Clear out "all registered frames" so we do not end up with large duplicated List
        this._allRegisteredFrames.Clear();

        topFrame.GenerateCode(trackingWriter, this, new MethodSourceWriter(trackingWriter, this, writer));

        this.WriteReturnStatement(writer);

        writer.FinishBlock();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var tempQualifier = this.Arguments.Select(x => x.Declaration);
        var arguments = string.Join(", ", tempQualifier);

        return $"public {this.ReturnType.FullNameInCode()} {this.MethodName}({arguments})";
    }

    private static Frame ChainFrames(IReadOnlyList<Frame> frames)
    {
        var visited = new List<Frame>();

        for (var i = 1; i < frames.Count; i++)
        {
            var previousFrame = frames[i - 1];
            var nextFrame = frames[i];

            if (visited.Contains(previousFrame))
            {
                throw new InvalidOperationException(
                    $"The frame '{previousFrame}' is duplicated in the method. Make sure each Frame instance is only added to the Frames collection once.");
            }

            previousFrame.NextFrame = nextFrame;

            visited.Add(previousFrame);
        }

        return frames[0];
    }

    private void WriteReturnStatement(ISourceWriter writer)
    {
        if (this.AsyncMode == AsyncMode.None && this.ReturnType == typeof(Task))
        {
            writer.WriteLine("return Task.CompletedTask;");
        }
    }

    private string DetermineReturnExpression()
    {
        return this.AsyncMode == AsyncMode.AsyncTask
            ? "async " + this.ReturnType.FullNameInCode()
            : this.ReturnType.FullNameInCode();
    }

    /// <inheritdoc />
    public Variable FindVariable(Type variableType)
    {
        var variable = ((IMethodVariables)this).TryFindVariable(variableType);

        if (variable == null)
        {
            var searchedSources = string.Join(", ", this.Sources.Select(s => s.GetType().Name));

            throw new ArgumentOutOfRangeException(
                nameof(variableType),
                $"Do not know how to build a variable of type '{variableType.FullName}'. Searched in argument list, constructor parameters and sources {searchedSources}");
        }

        return variable;
    }

    /// <inheritdoc />
    public Variable TryFindVariable(Type type)
    {
        if (this._variables.ContainsKey(type))
        {
            return this._variables[type];
        }

        var variable = this.DoFindVariable(type, 0);
        if (variable != null)
        {
            this._variables.Add(type, variable);
        }

        return variable;
    }

    /// <summary>
    /// Does the work to actually find a variable of the specified type, looking in <see cref="Arguments"/>,
    /// <see cref="Frames"/> and <see cref="Sources" />.
    /// </summary>
    /// <remarks>
    /// This method _may_ create variables/frames as required should the variable come from an <see cref="IVariableSource" />,
    /// so the creation should be cached (<seealso cref="_variables"/>).
    /// </remarks>
    /// <param name="variableType">The type of the variable to be found / created.</param>
    /// <param name="indentationLevel">The current indentation level, which will only be a value different to
    /// 0 when actually building this method's source.</param>
    /// <returns>A <see cref="Variable"/> of the given type.</returns>
    private Variable DoFindVariable(Type variableType, int indentationLevel)
    {
        foreach (var v in this.Arguments)
        {
            if (v.VariableType == variableType)
            {
                return v;
            }
        }

        // We try to find from all frames, and their children, a variable that is
        // created but ONLY IF it is created at a lower block/indentation level than
        // we are currently at (as otherwise it would not be visible due to block scope rules)
        Variable SearchExistingFramesForVariable(IEnumerable<Frame> frames)
        {
            foreach (var f in frames)
            {
                if (f.BlockLevel <= indentationLevel)
                {
                    foreach (var v in f.Creates)
                    {
                        if (v.VariableType == variableType)
                        {
                            return v;
                        }
                    }

                    if (f is CompositeFrame c)
                    {
                        var foundInner = SearchExistingFramesForVariable(c);

                        if (foundInner != null)
                        {
                            return foundInner;
                        }
                    }
                }
            }

            return null;
        }

        var fromFrames = SearchExistingFramesForVariable(this.Frames);

        if (fromFrames != null)
        {
            return fromFrames;
        }

        foreach (var s in this.Sources)
        {
            var created = s.TryFindVariable(this, variableType);

            if (created != null)
            {
                return created;
            }
        }

        return null;
    }

    internal void RegisterFrame(Frame frame)
    {
        this._allRegisteredFrames.Add(frame);
    }

    private class MethodSourceWriter : IMethodSourceWriter
    {
        private readonly IMethodVariables _variables;
        private readonly GeneratedMethod _method;
        private readonly ISourceWriter _inner;

        public MethodSourceWriter(IMethodVariables variables, GeneratedMethod method, ISourceWriter inner)
        {
            this._variables = variables;
            this._method = method;
            this._inner = inner;
        }

        public int IndentationLevel => this._inner.IndentationLevel;

        public ISourceWriter BlankLine()
        {
            this._inner.BlankLine();

            return this;
        }

        public ISourceWriter Indent()
        {
            this._inner.Indent();

            return this;
        }

        public ISourceWriter Block(string text)
        {
            this._inner.Block(text);

            return this;
        }

        public ISourceWriter Append(string text)
        {
            this._inner.Append(text);

            return this;
        }

        public ISourceWriter Append(char c)
        {
            this._inner.Append(c);

            return this;
        }

        public ISourceWriter WriteLines(string text = null)
        {
            this._inner.WriteLines(text);

            return this;
        }

        public ISourceWriter WriteLine(string text)
        {
            this._inner.WriteLine(text);

            return this;
        }

        public ISourceWriter FinishBlock(string extra = null)
        {
            this._inner.FinishBlock(extra);

            return this;
        }

        public void Write(Frame frame)
        {
            frame.GenerateCode(this._variables, this._method, this);
        }
    }

    /// <summary>
    /// Tracks indentation level and provides access to variables for a method, using the indentation
    /// level of the source writer to determine whether it would be possible to reuse a variable created
    /// in another frame.
    /// </summary>
    /// <remarks>
    /// This is NOT intended to be used as a real <see cref="ISourceWriter" /> as it does no processing or
    /// storage of any code, it is used purely for tracking indentation levels to provide better variable
    /// scoping support.
    /// </remarks>
    private class TrackingVariableWriter : ISourceWriter, IMethodVariables
    {
        private readonly GeneratedMethod _method;

        public TrackingVariableWriter(GeneratedMethod method)
        {
            this._method = method;
        }

        public int IndentationLevel { get; private set; }

        public ISourceWriter BlankLine()
        {
            return this;
        }

        public ISourceWriter Indent()
        {
            return this;
        }

        public ISourceWriter Block(string extra = null)
        {
            this.IndentationLevel++;

            return this;
        }

        public ISourceWriter FinishBlock(string extra = null)
        {
            this.IndentationLevel--;

            return this;
        }

        public ISourceWriter Append(string text)
        {
            return this;
        }

        public ISourceWriter Append(char c)
        {
            return this;
        }

        public ISourceWriter WriteLines(string text = null)
        {
            return this;
        }

        public ISourceWriter WriteLine(string text)
        {
            return this;
        }

        /// <inheritdoc />
        Variable IMethodVariables.FindVariable(Type variableType)
        {
            var variable = ((IMethodVariables)this).TryFindVariable(variableType);

            if (variable == null)
            {
                var searchedSources = string.Join(", ", this._method.Sources.Select(s => s.GetType().Name));

                var message = $"Could not find a variable of type '{variableType.FullName}' for method {this._method.GeneratedType.TypeName}.{this._method.MethodName}. " +
                              $"Searched in argument list, constructor parameters and sources {searchedSources}";

                throw new ArgumentOutOfRangeException(nameof(variableType), message);
            }

            return variable;
        }

        /// <inheritdoc />
        Variable IMethodVariables.TryFindVariable(Type type)
        {
            if (this._method._variables.ContainsKey(type))
            {
                return this._method._variables[type];
            }

            var variable = this._method.DoFindVariable(type, this.IndentationLevel);
            if (variable != null)
            {
                this._method._variables.Add(type, variable);
            }

            return variable;
        }
    }
}
