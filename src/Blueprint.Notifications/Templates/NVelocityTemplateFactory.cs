namespace Blueprint.Notifications.Templates
{
    /// <summary>
    /// An <see cref="ITemplateFactory"/> which will use NVelocity as the template
    /// engine.
    /// </summary>
    public class NVelocityTemplateFactory : ITemplateFactory
    {
        /// <summary>
        /// Creates a new NVelocity template, using the specified name and text.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="text">The text defining the template.</param>
        /// <returns>A new template instance using NVelocity.</returns>
        public Template CreateTemplate(string name, string text)
        {
            return new NVelocityTemplate(name, text);
        }
    }
}