namespace Blueprint.Notifications.Templates
{
    /// <summary>
    /// A factory which is used to construct a concrete implementation of
    /// a <see cref="Template"/>.
    /// </summary>
    public interface ITemplateFactory
    {
        /// <summary>
        /// Constructs a new template with the specified name and text.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="text">The text defining the template.</param>
        /// <returns>A new template instance.</returns>
        Template CreateTemplate(string name, string text);
    }
}
