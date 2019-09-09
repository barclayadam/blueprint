using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Core.Validation
{
    internal abstract class AttributeBasedValidatorFrame<T> : Frame
    {
        protected readonly OperationProperty Property;

        private Variable resultsVariable;

        protected AttributeBasedValidatorFrame(bool isAsync, OperationProperty property)
            : base(isAsync)
        {
            this.Property = property;
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            resultsVariable = chain.FindVariable(typeof(ValidationFailures));

            yield return resultsVariable;
        }

        protected void LoopAttributes(ISourceWriter writer, string methodCall)
        {
            var attributeType = typeof(T).FullNameInCode();
            var awaitMethod = IsAsync ? "await" : "";

            writer.WriteComment($"{Property.PropertyInfoVariable} == {Property.PropertyInfoVariable.Property.DeclaringType.Name}.{Property.PropertyInfoVariable.Property.Name}");
            writer.Write($"BLOCK:foreach (var attribute in {Property.PropertyAttributesVariable})");
            writer.Write($"BLOCK:if (attribute is {attributeType} x)");
            writer.Write($"var result = {awaitMethod} x.{methodCall};");
            writer.Write($"BLOCK:if (result != {Variable.StaticFrom<ValidationResult>(nameof(ValidationResult.Success))})");
            writer.Write($"{resultsVariable}.{nameof(ValidationFailures.AddFailure)}(result);");
            writer.FinishBlock();
            writer.FinishBlock();
            writer.FinishBlock();
        }
    }
}
