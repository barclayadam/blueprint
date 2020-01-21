using System;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    public class CommentFrame : SyncFrame
    {
        private readonly string commentText;

        public CommentFrame(string commentText)
        {
            this.commentText = commentText;
        }

        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            writer.WriteComment(commentText);

            next();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"// {commentText}";
        }
    }
}
