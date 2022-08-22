using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;
using Blueprint.Utilities;

namespace Blueprint.Validation;

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
        private readonly Variable _validationContextVariable;
        private readonly Variable _operationVariable;

        public ValidatableObjectFrame(Variable validationContextVariable, Variable operationVariable)
        {
            this._validationContextVariable = validationContextVariable;
            this._operationVariable = operationVariable;
        }

        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            var resultsVariable = variables.FindVariable(typeof(ValidationFailures));

            writer.Block($"foreach (var classFailure in (({typeof(IValidatableObject).FullNameInCode()}){this._operationVariable}).{nameof(IValidatableObject.Validate)}({this._validationContextVariable}))");
            writer.WriteLine($"{resultsVariable}.{nameof(ValidationFailures.AddFailure)}(classFailure);");
            writer.FinishBlock();

            next();
        }
    }

    private class DataAnnotationsValidatorFrame : AttributeBasedValidatorFrame<ValidationAttribute>
    {
        private readonly Variable _contextVariable;

        public DataAnnotationsValidatorFrame(Variable contextVariable, OperationProperty operationProperty)
            : base(false, operationProperty)
        {
            this._contextVariable = contextVariable;
        }

        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            writer.WriteLine($"{this._contextVariable}.MemberName = \"{this.Property.PropertyInfoVariable.Property.Name}\";");
            writer.WriteLine($"{this._contextVariable}.DisplayName = \"{this.Property.PropertyInfoVariable.Property.Name}\";");
            writer.BlankLine();

            this.LoopAttributes(
                variables,
                writer,
                $"{nameof(ValidationAttribute.GetValidationResult)}({this.Property.PropertyValueVariable}, {this._contextVariable})");

            next();
        }

        public override string ToString()
        {
            return $"[Validate property {this.Property.PropertyInfoVariable.Property.Name}]";
        }
    }
}