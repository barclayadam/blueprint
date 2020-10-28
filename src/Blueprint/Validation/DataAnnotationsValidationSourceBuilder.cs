using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Utilities;

namespace Blueprint.Validation
{
    public class DataAnnotationsValidationSourceBuilder : IValidationSourceBuilder
    {
        /// <inheritdoc />
        public IEnumerable<Frame> GetFrames(Variable operationVariable, List<OperationProperty> properties)
        {
            var contextCreator =
                new ConstructorFrame<ValidationContext>(() => new ValidationContext(null))
                {
                    Parameters = { [0] = operationVariable },
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
                        yield return new DataAnnotationsValidatorFrame(contextCreator.Variable, p);
                    }
                }
            }
        }

        private class ValidatableObjectFrame : SyncFrame
        {
            private readonly Variable validationContextVariable;
            private readonly Variable operationVariable;

            public ValidatableObjectFrame(Variable validationContextVariable, Variable operationVariable)
            {
                this.validationContextVariable = validationContextVariable;
                this.operationVariable = operationVariable;
            }

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var resultsVariable = variables.FindVariable(typeof(ValidationFailures));

                writer.Block($"foreach (var classFailure in (({typeof(IValidatableObject).FullNameInCode()}){operationVariable}).{nameof(IValidatableObject.Validate)}({validationContextVariable}))");
                writer.WriteLine($"{resultsVariable}.{nameof(ValidationFailures.AddFailure)}(classFailure);");
                writer.FinishBlock();

                next();
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

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                writer.WriteLine($"{contextVariable}.MemberName = \"{Property.PropertyInfoVariable.Property.Name}\";");
                writer.WriteLine($"{contextVariable}.DisplayName = \"{Property.PropertyInfoVariable.Property.Name}\";");
                writer.BlankLine();

                LoopAttributes(
                    variables,
                    writer,
                    $"{nameof(ValidationAttribute.GetValidationResult)}({Property.PropertyValueVariable}, {contextVariable})");

                next();
            }

            public override string ToString()
            {
                return $"[Validate property {Property.PropertyInfoVariable.Property.Name}]";
            }
        }
    }
}
