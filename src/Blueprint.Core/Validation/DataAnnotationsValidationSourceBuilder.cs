using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Blueprint.Core.Utilities;

using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Core.Validation
{
    public class DataAnnotationsValidationSourceBuilder : IValidationSourceBuilder
    {
        /// <inheritdoc />
        public IEnumerable<Frame> GetFrames(Variable operationVariable, List<OperationProperty> properties)
        {
            var contextCreator =
                new ConstructorFrame<ValidationContext>(() => new ValidationContext(null))
                {
                    Parameters = { [0] = operationVariable }
                };

            var hasClassLevel = typeof(IValidatableObject).IsAssignableFrom(operationVariable.VariableType);
            var hasPropertyLevel = properties.Any(p =>
                p.PropertyInfoVariable.Property.GetAttributes<ValidationAttribute>(true).Any());

            if (hasClassLevel || hasPropertyLevel)
            {
                yield return contextCreator;
            }

            if (hasClassLevel)
            {
                yield return new ValidatableObjectFrame(contextCreator.Variable, operationVariable);
            }

            if (hasPropertyLevel)
            {
                foreach (var p in properties)
                {
                    var attributes = p.PropertyInfoVariable.Property.GetAttributes<ValidationAttribute>(true);

                    if (attributes.Any())
                    {
                        yield return contextCreator;
                        yield return new DataAnnotationsValidatorFrame(contextCreator.Variable, p);
                    }
                }
            }
        }

        private class ValidatableObjectFrame : SyncFrame
        {
            private readonly Variable validationContextVariable;
            private readonly Variable operationVariable;
            private Variable resultsVariable;

            public ValidatableObjectFrame(Variable validationContextVariable, Variable operationVariable)
            {
                this.validationContextVariable = validationContextVariable;
                this.operationVariable = operationVariable;
            }

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                writer.Write($"BLOCK:foreach (var classFailure in (({typeof(IValidatableObject).FullNameInCode()}){operationVariable}).{nameof(IValidatableObject.Validate)}({validationContextVariable}))");
                writer.Write($"{resultsVariable}.{nameof(ValidationFailures.AddFailure)}(classFailure);");
                writer.FinishBlock();

                Next?.GenerateCode(method, writer);
            }

            public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
            {
                resultsVariable = chain.FindVariable(typeof(ValidationFailures));

                yield return resultsVariable;
            }
        }

        private class DataAnnotationsValidatorFrame : AttributeBasedValidatorFrame<ValidationAttribute>
        {
            private readonly Variable contextVariable;

            public DataAnnotationsValidatorFrame(Variable contextVariable, OperationProperty operationProperty)
                : base(false, operationProperty)
            {
                this.contextVariable = contextVariable;
            }

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                writer.Write($"{contextVariable}.MemberName = \"{Property.PropertyInfoVariable.Property.Name}\";");
                writer.Write($"{contextVariable}.DisplayName = \"{Property.PropertyInfoVariable.Property.Name}\";");
                writer.BlankLine();

                LoopAttributes(writer, $"{nameof(ValidationAttribute.GetValidationResult)}({Property.PropertyValueVariable}, {contextVariable})");

                Next?.GenerateCode(method, writer);
            }
        }
    }
}
