using System;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames;

/// <summary>
/// A <see cref="SyncFrame" /> that writes a comment.
/// </summary>
public class CommentFrame : SyncFrame
{
    private readonly string _commentText;

    /// <summary>
    /// Initialises a new instance of the <see cref="CommentFrame" /> class.
    /// </summary>
    /// <param name="commentText">The comment to write.</param>
    public CommentFrame(string commentText)
    {
        this._commentText = commentText;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"// {this._commentText}";
    }

    /// <inheritdoc />
    protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
    {
        writer.Comment(this._commentText);

        next();
    }
}