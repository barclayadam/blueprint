using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Blueprint.Validation
{
    public class ValidationFailures
    {
        public const string FormLevelPropertyName = "form";

        public static readonly ValidationFailures None = new ValidationFailures();

        private readonly Dictionary<string, IEnumerable<string>> failures = new Dictionary<string, IEnumerable<string>>();

        public int Count => failures.Values.Count;

        public void AddFailure(ValidationResult validationResult)
        {
            if (!validationResult.MemberNames.Any())
            {
                AddFailure(FormLevelPropertyName, validationResult.ErrorMessage);
            }
            else
            {
                foreach (var memberName in validationResult.MemberNames)
                {
                    AddFailure(memberName, validationResult.ErrorMessage);
                }
            }
        }

        public ValidationFailures AddFailure(string propertyName, string errorMessage)
        {
            if (!failures.TryGetValue(propertyName, out var errors))
            {
                errors = new List<string>(2);
                failures[propertyName] = errors;
            }

            ((List<string>)errors).Add(errorMessage);

            return this;
        }

        public Dictionary<string, IEnumerable<string>> AsDictionary()
        {
            return failures;
        }
    }
}
