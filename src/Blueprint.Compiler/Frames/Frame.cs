using System;
using System.Collections.Generic;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    public abstract class Frame
    {
        private readonly List<Variable> _creates = new List<Variable>();
        private readonly HashSet<Variable> _uses = new HashSet<Variable>();

        private IMethodSourceWriter _writer;
        private GeneratedMethod _method;
        private IMethodVariables _variables;

        /// <summary>
        /// Initialises a new instance of the <see cref="Frame" /> class.
        /// </summary>
        /// <param name="isAsync">Whether this <see cref="Frame"/> is async.</param>
        protected Frame(bool isAsync)
        {
            this.IsAsync = isAsync;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Frame" /> is async.
        /// </summary>
        public virtual bool IsAsync { get; }

        /// <summary>
        /// Gets the <see cref="Variable" />s used by this <see cref="Frame" />.
        /// </summary>
        public IEnumerable<Variable> Uses => this._uses;

        /// <summary>
        /// Gets the variables that <b>this</b> <see cref="Frame" /> is responsible for creating, NOT
        /// returning any child variables if this is a <see cref="CompositeFrame" />.
        /// </summary>
        public IEnumerable<Variable> Creates => this._creates;

        /// <summary>
        /// Gets the block level of the frame, indicating how many scopes/blocks 'deep' this
        /// <see cref="Frame" /> is (i.e. starts at 0, a try block is started, all inner frames are then 1).
        /// </summary>
        public int BlockLevel { get; private set; }

        internal Frame NextFrame { get; set; }

        /// <summary>
        /// Generates the code required by this <see cref="Frame"/>, delegating the responsibility to the
        /// abstract method <see cref="Generate" />.
        /// </summary>
        /// <remarks>
        /// This method will store the parameters and the current <see cref="ISourceWriter.IndentationLevel" /> inside
        /// of this <see cref="Frame" /> for later referencing.
        /// </remarks>
        /// <param name="variables">A source of variable for a method, from which to grab any <see cref="Variable"/>s that this
        ///     frame needs but does not create.</param>
        /// <param name="method">The method this <see cref="Frame"/> belongs to.</param>
        /// <param name="writer">Where to write the code to.</param>
        public void GenerateCode(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer)
        {
            this.BlockLevel = writer.IndentationLevel;

            this._variables = variables;
            this._method = method;
            this._writer = writer;

            method.RegisterFrame(this);

            this.Generate(new MethodVariableUsageRecorder(variables, this._uses), method, writer, this.Next);
        }

        public virtual bool CanReturnTask()
        {
            return false;
        }

        /// <summary>
        /// Implements the actual generation of the code for this <see cref="Frame" />, writing to
        /// the given <see cref="ISourceWriter" />.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="GeneratedMethod" /> given is the same as <see cref="_method" />, but is provided as
        /// a convenience parameter to make it more obvious it's available.
        /// </para>
        /// <para>
        /// The <see cref="ISourceWriter" /> given is the same as <see cref="_writer" />, but is provided as
        /// a convenience parameter to make it more obvious it's available.
        /// </para>
        /// <para>
        /// Frames <em>should</em> typically call <paramref name="next" /> to insert code from the next frame. If they
        /// do not then no further code is executed and this <see cref="Frame" /> therefore becomes the last frame
        /// of the method.
        /// </para>
        /// </remarks>
        /// <param name="variables">A source of variables, used to grab from other frames / <see cref="IVariableSource"/>s the
        ///     variables needed to generate the code.</param>
        /// <param name="method">The method to which this <see cref="Frame" /> belongs.</param>
        /// <param name="writer">The writer to write code to.</param>
        /// <param name="next">The action to call to write the next frame (equivalent to calling <see cref="Next"/> directly).</param>
        protected abstract void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next);

        /// <summary>
        /// Generates the code for the next frame, if one exists.
        /// </summary>
        private void Next()
        {
            this.NextFrame?.GenerateCode(this._variables, this._method, this._writer);
        }

        internal void AddCreates(Variable variable)
        {
            this._creates.Add(variable);
        }

        internal void AddUses(Variable variable)
        {
            this._uses.Add(variable);
        }

        private class MethodVariableUsageRecorder : IMethodVariables
        {
            private readonly IMethodVariables _inner;
            private readonly HashSet<Variable> _uses;

            public MethodVariableUsageRecorder(IMethodVariables inner, HashSet<Variable> uses)
            {
                this._inner = inner;
                this._uses = uses;
            }

            public Variable FindVariable(Type type)
            {
                return this.Record(this._inner.FindVariable(type));
            }

            public Variable TryFindVariable(Type type)
            {
                return this.Record(this._inner.TryFindVariable(type));
            }

            private Variable Record(Variable v)
            {
                if (v != null)
                {
                    this._uses.Add(v);

                    foreach (var dependency in v.Dependencies)
                    {
                        this.Record(dependency);
                    }
                }

                return v;
            }
        }
    }
}
