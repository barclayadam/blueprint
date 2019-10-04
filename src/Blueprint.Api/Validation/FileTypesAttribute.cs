using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NJsonSchema;

namespace Blueprint.Api.Validation
{
    /// <summary>
    /// Causes a validation error if a file is of the correct type (based on file name extension only).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FileTypesAttribute : ValidationAttribute, IOpenApiValidationAttribute
    {
        private readonly string[] validExtensions;

        public FileTypesAttribute(params string[] validExtensions)
        {
            this.validExtensions = validExtensions ?? throw new ArgumentException(nameof(validExtensions));
        }

        public string ValidatorKeyword => "x-validator-file-types";

        public Task PopulateAsync(JsonSchema4 schema, ApiOperationContext apiOperationContext)
        {
            schema.ExtensionData[ValidatorKeyword] = validExtensions;

            return Task.CompletedTask;
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

            if (validExtensions == null || validExtensions.Length == 0)
            {
                throw new ArgumentException(nameof(validExtensions));
            }

            var file = (Base64FileData)value;
            var extension = Path.GetExtension(file.FileName);

            if (validExtensions.Contains(extension))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), new[] { validationContext.DisplayName });
        }
    }
}
