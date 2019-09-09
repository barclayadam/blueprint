using System.Collections.Generic;
using System.Linq;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Core.Utilities;

namespace Blueprint.Api.Validation
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
            private Variable contextVariable;

            public BlueprintValidatorFrame(OperationProperty property) : base(true, property)
            {
            }

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                LoopAttributes(
                    writer,
                    $"{nameof(BlueprintValidationAttribute.GetValidationResultAsync)}({Property.PropertyValueVariable}, \"{Property.PropertyInfoVariable.Property.Name}\", {contextVariable})");

                Next?.GenerateCode(method, writer);
            }

            public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
            {
                contextVariable = chain.FindVariable(typeof(ApiOperationContext));

                yield return contextVariable;

                foreach (var v in base.FindVariables(chain))
                {
                    yield return v;
                }
            }
        }
    }
}
