using System.Diagnostics;
using System.Text;

using NLog;
using NLog.LayoutRenderers;

namespace Blueprint.Core.Tracing
{
    /// <summary>
    /// A layout renderer to be used with NLog that will output the value of <see cref="Activity.Id"/> from <see cref="Activity.Current" />.
    /// </summary>
    [LayoutRenderer("request-id")]
    public class RequestIdLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Will append the value of <see cref="Activity.Id"/> from <see cref="Activity.Current" />.
        /// </summary>
        /// <param name="builder">The builder to which the request identifier will be appended.</param>
        /// <param name="logEvent">The log event that has caused this layout renderer to be executed, ignore.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (Activity.Current != null)
            {
                builder.AppendFormat("req_id={0}", Activity.Current.Id);
            }
        }
    }
}