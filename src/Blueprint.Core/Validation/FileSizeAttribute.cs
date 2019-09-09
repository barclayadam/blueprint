using System.Threading.Tasks;

namespace Blueprint.Core.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using Api;

    using NJsonSchema;

    using OpenApi;

    /// <summary>
    /// Causes a validation error if a file is over a given size or zero bytes
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FileSizeAttribute : ValidationAttribute, IOpenApiValidationAttribute
    {
        private readonly long maxSize;
        public string ValidatorKeyword => "x-validator-file-size";

        public FileSizeAttribute(long maxSize)
        {
            this.maxSize = maxSize;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value.GetType() != typeof(Base64FileData))
            {
                throw new InvalidOperationException($"This attribute can only be applied to {nameof(Base64FileData)}");
            }

            var file = (Base64FileData)value;
            if (file.Data.Length > 0 && file.Data.Length <= maxSize)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), new[] { validationContext.DisplayName });
        }

        public async ValueTask PopulateAsync(JsonSchema4 schema, ApiOperationContext apiOperationContext)
        {
            schema.ExtensionData[this.ValidatorKeyword] = maxSize;
        }
    }
}
