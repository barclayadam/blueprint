using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames;

/// <summary>
/// Represents a "composite frame", a frame that contains other <see cref="Frame" /> instances.
/// </summary>
public abstract class CompositeFrame : Frame, IEnumerable<Frame>
{
    private readonly List<Frame> _inner;

    /// <summary>
    /// Initialises a new instance of the <see cref="CompositeFrame"/> class.
    /// </summary>
    /// <param name="inner">A set of <see cref="Frame" />s that make up this one.</param>
    protected CompositeFrame(params Frame[] inner)
        : base(inner.Any(x => x.IsAsync))
    {
        this._inner = inner.ToList();
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="CompositeFrame"/> is async, determined by whether
    /// any of the children <see cref="Frame.IsAsync"/> properties are <c>true</c>.
    /// </summary>
    public override bool IsAsync => this._inner.Any(i => i.IsAsync);

    /// <summary>
    /// The <see cref="Frame" />s that compose up to this one.
    /// </summary>
    public IReadOnlyList<Frame> Inner => this._inner;

    /// <summary>
    /// Appends a new <see cref="Frame" /> to the end of this composite frame.
    /// </summary>
    /// <param name="innerFrame">The frame to add.</param>
    public void Add(Frame innerFrame)
    {
        this._inner.Add(innerFrame);
    }

    /// <summary>
    /// Returns the value of <see cref="Frame.CanReturnTask" /> from the last frame.
    /// </summary>
    /// <returns>Whether this frame can return a <see cref="Task" /></returns>
    public override bool CanReturnTask()
    {
        return this._inner.Last().CanReturnTask();
    }

    /// <inheritdoc/>
    protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
    {
        if (this._inner.Count > 1)
        {
            for (var i = 1; i < this._inner.Count; i++)
            {
                this._inner[i - 1].NextFrame = this._inner[i];
            }
        }

        this.GenerateCode(variables, method, writer, this._inner[0]);

        next();
    }

    /// <inheritdoc/>
    IEnumerator<Frame> IEnumerable<Frame>.GetEnumerator()
    {
        return this._inner.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this._inner.GetEnumerator();
    }

    /// <summary>
    /// Generates the code for this composite frame, with the given inner <see cref="Frame" /> having been composed such
    /// that if written it will write <strong>all</strong> inner frames at once.
    /// </summary>
    /// <remarks>
    /// Typically implementations will write a prefix (i.f. an opening if statement), call <c>inner.GenerateCode(...)</c> and then
    /// finally "close" the statement.
    /// </remarks>
    /// <param name="variables">The method's variable source.</param>
    /// <param name="method">The method this frame is being written to.</param>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="inner">The first inner <see cref="Frame" /> that should be written.</param>
    protected abstract void GenerateCode(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Frame inner);
}