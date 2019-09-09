namespace Blueprint.Api
{
    /// <summary>
    /// Represents a property validator, a simple description of a validator that could
    /// be persisted for a client-side validation library to consume.
    /// </summary>
    public class PropertyValidator
    {
        /// <summary>
        /// Gets or sets the validation message, which may be <c>null</c>, that
        /// should be shown in the case of a validation failure.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the name of the validator (e.g. 'regex', 'stringLength').
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parameter for this validator, used to provide further options
        /// to the client-side validator (e.g. the maximum length when specifying a
        /// string length validator).
        /// </summary>
        public string Parameter { get; set; }
    }
}