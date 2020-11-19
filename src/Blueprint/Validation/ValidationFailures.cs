using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Blueprint.Validation
{
    public class ValidationFailures
    {
        public const string FormLevelPropertyName = "form";

        public static readonly ValidationFailures None = new ValidationFailures();

        private readonly Dictionary<string, IEnumerable<string>> _failures = new Dictionary<string, IEnumerable<string>>();

        public int Count => this._failures.Values.Count;

        public void AddFailure(ValidationResult validationResult)
        {
            if (!validationResult.MemberNames.Any())
            {
                this.AddFailure(FormLevelPropertyName, validationResult.ErrorMessage);
            }
            else
            {
                foreach (var memberName in validationResult.MemberNames)
                {
                    this.AddFailure(memberName, validationResult.ErrorMessage);
                }
            }
        }

        public ValidationFailures AddFailure(string propertyName, string errorMessage)
        {
            if (!this._failures.TryGetValue(propertyName, out var errors))
            {
                errors = new List<string>(2);
                this._failures[propertyName] = errors;
            }

            ((List<string>)errors).Add(errorMessage);

            return this;
        }

        public Dictionary<string, IEnumerable<string>> AsDictionary()
        {
            return this._failures;
        }
    }
}
