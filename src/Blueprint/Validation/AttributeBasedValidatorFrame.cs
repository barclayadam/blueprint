using System.ComponentModel.DataAnnotations;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;

namespace Blueprint.Validation;

internal abstract class AttributeBasedValidatorFrame<T> : Frame
{
    private readonly OperationProperty _property;

    protected AttributeBasedValidatorFrame(bool @is, OperationProperty property)
        : base(@is)
    {
        this._property = property;
    }

    protected OperationProperty Property => this._property;

    protected void LoopAttributes(IMethodVariables variables, ISourceWriter writer, string methodCall)
    {
        var resultsVariable = variables.FindVariable(typeof(ValidationFailures));
        var attributeType = typeof(T).FullNameInCode();
        var awaitMethod = this.IsAsync ? "await" : string.Empty;

        writer.Comment($"{this._property.PropertyInfoVariable} == {this._property.PropertyInfoVariable.Property.DeclaringType.Name}.{this._property.PropertyInfoVariable.Property.Name}");
        writer.Block($"foreach (var attribute in {this._property.PropertyAttributesVariable})");
        writer.Block($"if (attribute is {attributeType} x)");
        writer.WriteLine($"var result = {awaitMethod} x.{methodCall};");
        writer.Block($"if (result != {Variable.StaticFrom<ValidationResult>(nameof(ValidationResult.Success))})");
        writer.WriteLine($"{resultsVariable}.{nameof(ValidationFailures.AddFailure)}(result);");
        writer.FinishBlock();
        writer.FinishBlock();
        writer.FinishBlock();
    }
}