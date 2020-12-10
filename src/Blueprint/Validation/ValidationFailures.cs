using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Blueprint.Validation
{
    /// <summary>
    /// A class used to collect validation failures.
    /// </summary>
    public class ValidationFailures
    {
        private static readonly Dictionary<string, IEnumerable<string>> _noFailures = new Dictionary<string, IEnumerable<string>>();

        /// <summary>
        /// The key that should be used for an overall "form level" message, one that is not associated with
        /// a particular property of a message/
        /// </summary>
        public const string FormLevelPropertyName = "form";

        /// <summary>
        /// Represents no failures.
        /// </summary>
        public static readonly ValidationFailures None = new ValidationFailures();

        // Note we do not create this up-front as the assumption is most operations will NOT result in any failures
        // and therefore this dictionary will never be populated.
        private Dictionary<string, IEnumerable<string>> _failures;

        /// <summary>
        /// Gets the number of errors that have been recorded.
        /// </summary>
        public int Count => this._failures?.Values.Count ?? 0;

        /// <summary>
        /// Adds a new <see cref="ValidationResult" /> to this failures collection.
        /// </summary>
        /// <param name="validationResult">The result to add.</param>
        public void AddFailure(ValidationResult validationResult)
        {
            Guard.NotNull(nameof(validationResult), validationResult);

            this._failures ??= new Dictionary<string, IEnumerable<string>>();

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

        /// <summary>
        /// Adds a new failure for the given property.
        /// </summary>
        /// <param name="propertyName">The name of the property this failure is for.</param>
        /// <param name="errorMessage">The human-readable error message.</param>
        public void AddFailure(string propertyName, string errorMessage)
        {
            Guard.NotNull(nameof(propertyName), propertyName);
            Guard.NotNull(nameof(errorMessage), errorMessage);

            this._failures ??= new Dictionary<string, IEnumerable<string>>();

            if (!this._failures.TryGetValue(propertyName, out var errors))
            {
                errors = new List<string>(2);
                this._failures[propertyName] = errors;
            }

            ((List<string>)errors).Add(errorMessage);
        }

        /// <summary>
        /// Returns this collection as a dictionary of property name to a list of errors.
        /// </summary>
        /// <returns>This collection as a dictionary.</returns>
        public IReadOnlyDictionary<string, IEnumerable<string>> AsDictionary()
        {
            return this._failures ?? _noFailures;
        }
    }
}
