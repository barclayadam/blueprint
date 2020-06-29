using Blueprint;

namespace Blueprint.Notifications.Templates
{
    /// <summary>
    /// Represents a loaded template.
    /// </summary>
    public abstract class Template
    {
        private readonly string name;

        private readonly string text;

        /// <summary>
        /// Initializes a new instance of the Template class.
        /// </summary>
        /// <param name="name">The name of this template, which can be used if logging errors.</param>
        /// <param name="text">The text of this template.</param>
        protected Template(string name, string text)
        {
            Guard.NotNullOrEmpty("name", name);
            Guard.NotNullOrEmpty("text", text);

            this.text = text;
            this.name = name;
        }

        /// <summary>
        /// Gets the name of this template, which can be used if logging errors.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Gets the text of this template.
        /// </summary>
        public string Text => text;

        /// <summary>
        /// Transforms this template into a final string representation, having merged in any
        /// values to be replaced within the template text.
        /// </summary>
        /// <param name="values">The values that are to be merged into the template.</param>
        /// <returns>A final representation of the template, with the values having been injected.</returns>
        public abstract string Merge(TemplateValues values);
    }
}
