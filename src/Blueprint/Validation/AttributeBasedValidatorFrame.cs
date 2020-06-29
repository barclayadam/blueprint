using System.ComponentModel.DataAnnotations;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Validation
{
    internal abstract class AttributeBasedValidatorFrame<T> : Frame
    {
        private readonly OperationProperty property;

        protected AttributeBasedValidatorFrame(bool isAsync, OperationProperty property)
            : base(isAsync)
        {
            this.property = property;
        }

        protected OperationProperty Property => property;

        protected void LoopAttributes(IMethodVariables variables, ISourceWriter writer, string methodCall)
        {
            var resultsVariable = variables.FindVariable(typeof(ValidationFailures));
            var attributeType = typeof(T).FullNameInCode();
            var awaitMethod = IsAsync ? "await" : string.Empty;

            writer.WriteComment($"{property.PropertyInfoVariable} == {property.PropertyInfoVariable.Property.DeclaringType.Name}.{property.PropertyInfoVariable.Property.Name}");
            writer.Block($"foreach (var attribute in {property.PropertyAttributesVariable})");
            writer.Block($"if (attribute is {attributeType} x)");
            writer.Write($"var result = {awaitMethod} x.{methodCall};");
            writer.Block($"if (result != {Variable.StaticFrom<ValidationResult>(nameof(ValidationResult.Success))})");
            writer.Write($"{resultsVariable}.{nameof(ValidationFailures.AddFailure)}(result);");
            writer.FinishBlock();
            writer.FinishBlock();
            writer.FinishBlock();
        }
    }
}
