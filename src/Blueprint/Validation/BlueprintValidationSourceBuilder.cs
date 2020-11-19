using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Utilities;

namespace Blueprint.Validation
{
    public class BlueprintValidationSourceBuilder : IValidationSourceBuilder
    {
        /// <inheritdoc />
        public IEnumerable<Frame> GetFrames(Variable operationVariable, List<OperationProperty> properties)
        {
            foreach (var p in properties)
            {
                var attributes = p.PropertyInfoVariable.Property.GetAttributes<BlueprintValidationAttribute>(true);

                if (attributes.Any())
                {
                    yield return new BlueprintValidatorFrame(p);
                }
            }
        }

        private class BlueprintValidatorFrame : AttributeBasedValidatorFrame<BlueprintValidationAttribute>
        {
            public BlueprintValidatorFrame(OperationProperty property) : base(true, property)
            {
            }

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var contextVariable = variables.FindVariable(typeof(ApiOperationContext));

                this.LoopAttributes(
                    variables,
                    writer,
                    $"{nameof(BlueprintValidationAttribute.GetValidationResultAsync)}({this.Property.PropertyValueVariable}, \"{this.Property.PropertyInfoVariable.Property.Name}\", {contextVariable})");

                next();
            }
        }
    }
}
