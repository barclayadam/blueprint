using Blueprint.Compiler.Model;

namespace Blueprint.Api.Validation
{
    public class OperationProperty
    {
        public PropertyInfoVariable PropertyInfoVariable { get; set; }

        public Variable PropertyAttributesVariable { get; set; }

        public Variable PropertyValueVariable { get; set; }
    }
}
