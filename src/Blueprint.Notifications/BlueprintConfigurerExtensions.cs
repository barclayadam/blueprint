using Blueprint.Api;
using Blueprint.Notifications.Notifications;
using Blueprint.Notifications.Notifications.Handlers;
using Blueprint.Notifications.Templates;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Notifications
{
    public static class BlueprintConfigurerExtensions
    {
        public static BlueprintConfigurer AddNotifications(this BlueprintConfigurer configurer)
        {
            configurer.Services.AddTransient<INotificationRepository, EmbeddedResourceNotificationRepository>();
            configurer.Services.AddTransient<INotificationService, NotificationService>();
            configurer.Services.AddTransient<INotificationHandler, TemplatedEmailHandler>();
            configurer.Services.AddTransient<ITemplateFactory, NVelocityTemplateFactory>();
            configurer.Services.AddTransient<INotificationRepository, EmbeddedResourceNotificationRepository>();
            configurer.Services.AddTransient<IEmailSender, SmtpClientEmailSender>();

            return configurer;
        }
    }
}
