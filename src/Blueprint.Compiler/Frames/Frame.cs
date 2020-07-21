using System;
using System.Collections.Generic;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Frames
{
    public abstract class Frame
    {
        private readonly List<Variable> creates = new List<Variable>();
        private readonly HashSet<Variable> uses = new HashSet<Variable>();

        private IMethodSourceWriter writer;
        private GeneratedMethod method;
        private IMethodVariables variables;

        /// <summary>
        /// Initialises a new instance of the <see cref="Frame" /> class.
        /// </summary>
        /// <param name="isAsync">Whether this <see cref="Frame"/> is async.</param>
        protected Frame(bool isAsync)
        {
            IsAsync = isAsync;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Frame" /> is async.
        /// </summary>
        public virtual bool IsAsync { get; }

        /// <summary>
        /// Gets the <see cref="Variable" />s used by this <see cref="Frame" />.
        /// </summary>
        public IEnumerable<Variable> Uses => uses;

        /// <summary>
        /// Gets the variables that <b>this</b> <see cref="Frame" /> is responsible for creating, NOT
        /// returning any child variables if this is a <see cref="CompositeFrame" />.
        /// </summary>
        public IEnumerable<Variable> Creates => creates;

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
            BlockLevel = writer.IndentationLevel;

            this.variables = variables;
            this.method = method;
            this.writer = writer;

            method.RegisterFrame(this);

            Generate(new MethodVariableUsageRecorder(variables, uses), method, writer, Next);
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
        /// The <see cref="GeneratedMethod" /> given is the same as <see cref="method" />, but is provided as
        /// a convenience parameter to make it more obvious it's available.
        /// </para>
        /// <para>
        /// The <see cref="ISourceWriter" /> given is the same as <see cref="writer" />, but is provided as
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
            NextFrame?.GenerateCode(variables, method, writer);
        }

        internal void AddCreates(Variable variable)
        {
            creates.Fill(variable);
        }

        internal void AddUses(Variable variable)
        {
            uses.Add(variable);
        }

        private class MethodVariableUsageRecorder : IMethodVariables
        {
            private readonly IMethodVariables inner;
            private readonly HashSet<Variable> uses;

            public MethodVariableUsageRecorder(IMethodVariables inner, HashSet<Variable> uses)
            {
                this.inner = inner;
                this.uses = uses;
            }

            public Variable FindVariable(Type type)
            {
                return Record(inner.FindVariable(type));
            }

            public Variable TryFindVariable(Type type)
            {
                return Record(inner.TryFindVariable(type));
            }

            private Variable Record(Variable v)
            {
                if (v != null)
                {
                    uses.Add(v);

                    foreach (var dependency in v.Dependencies)
                    {
                        Record(dependency);
                    }
                }

                return v;
            }
        }
    }
}
