﻿using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Blueprint.Validation;

/// <summary>
/// An implementation of <see cref="IValidator"/> that uses the .NET DataAnnotations framework
/// to validate the public properties of the object being validated.
/// </summary>
public class BlueprintValidator : IValidator
{
    private readonly IEnumerable<IValidationSource> _validationSources;

    public BlueprintValidator(IEnumerable<IValidationSource> validationSources)
    {
        this._validationSources = validationSources;
    }

    /// <inheritdoc />
    public async Task<ValidationFailures> GetValidationResultsAsync(object value, ApiOperationContext apiOperationContext)
    {
        var results = new ValidationFailures();
        PropertyInfo[] propertyInfos = null;

        foreach (var validationSource in this._validationSources)
        {
            if (validationSource is IAttributeValidationSource attributeValidationSource)
            {
                if (propertyInfos == null)
                {
                    propertyInfos = value.GetType().GetProperties();
                }

                foreach (var propertyInfo in propertyInfos)
                {
                    await attributeValidationSource.AddAttributeValidationResultsAsync(propertyInfo, value, apiOperationContext, results);
                }
            }

            if (validationSource is IClassValidationSource classValidationSource)
            {
                await classValidationSource.AddClassValidationResultsAsync(value, apiOperationContext, results);
            }
        }

        return results;
    }
}