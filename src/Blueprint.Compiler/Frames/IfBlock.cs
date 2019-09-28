using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    public class IfBlock : CompositeFrame
    {
        private readonly string condition;

        public IfBlock(string condition, params Frame[] inner) : base(inner)
        {
            this.condition = condition;
        }

        public IfBlock(Variable variable, params Frame[] inner) : this(variable.Usage, inner)
        {
        }

        protected override void GenerateCode(GeneratedMethod method, ISourceWriter writer, Frame inner)
        {
            writer.Write($"BLOCK:if ({condition})");
            inner.GenerateCode(method, writer);
            writer.FinishBlock();
        }
    }
}
