using System.Diagnostics.CodeAnalysis;
using Blueprint.Notifications.Notifications;
using Blueprint.Notifications.Notifications.Handlers;
using Blueprint.Notifications.Templates;
using StructureMap;

namespace Blueprint.StructureMap
{
    public class BlueprintNotificationRegistry : Registry
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "StructureMap registry by definition has high class coupling")]
        public BlueprintNotificationRegistry()
        {
            For<INotificationRepository>().Use<EmbeddedResourceNotificationRepository>();

            For<INotificationService>().Use<NotificationService>();
            For<INotificationHandler>().Use<TemplatedEmailHandler>();
            For<ITemplateFactory>().Use<NVelocityTemplateFactory>();
            For<IEmailSender>().Use<SmtpClientEmailSender>();
        }
    }
}
