using System.Globalization;
using System.IO;
using Commons.Collections;
using NLog;
using NVelocity;
using NVelocity.App;
using NVelocity.Exception;

namespace Blueprint.Core.Templates
{
    /// <summary>
    /// An implementation of an <see cref="ITemplateFactory"/> which uses NVelocity as its
    /// template language.
    /// </summary>
    public class NVelocityTemplate : Template
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the NVelocityTemplate class.
        /// </summary>
        /// <param name="name">The name of this template, which can be used if logging errors.</param>
        /// <param name="text">The text of this template.</param>
        internal NVelocityTemplate(string name, string text)
                : base(name, text)
        {
            Guard.NotNullOrEmpty("name", name);
            Guard.NotNullOrEmpty("text", text);
        }

        /// <summary>
        /// Transforms this template into a final string representation, having merged in any
        /// values to be replaced within the template text.
        /// </summary>
        /// <param name="values">The values that are to be merged into the template.</param>
        /// <returns>A final representation of the template, with the values having been injected.</returns>
        public override string Merge(TemplateValues values)
        {
            var engine = CreateEngine();
            var context = CreateContext(values);

            return Evaluate(context, engine);
        }

        private static VelocityContext CreateContext(TemplateValues values)
        {
            var context = new VelocityContext();

            foreach (var item in values)
            {
                context.Put(item.Key, item.Value);
            }

            return context;
        }

        private static VelocityEngine CreateEngine()
        {
            return new VelocityEngine(new ExtendedProperties());
        }

        private string Evaluate(VelocityContext context, VelocityEngine engine)
        {
            using (var textWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                try
                {
                    engine.Evaluate(context, textWriter, Name, Text);
                }
                catch (ParseErrorException e)
                {
                    Log.Error(e, "Parsing of the template '{0}' has failed. Template was '{1}'.", Name, Text);
                    
                    throw;
                }

                return textWriter.ToString();
            }
        }
    }
}