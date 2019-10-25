using Blueprint.Api.Configuration;
using Blueprint.Notifications.Handlers;
using Blueprint.Notifications.Templates;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Notifications
{
    public static class BlueprintConfigurerExtensions
    {
        public static BlueprintApiConfigurer AddNotifications(this BlueprintApiConfigurer blueprintApiConfigurer)
        {
            blueprintApiConfigurer.Services.AddOptions<TemplatedEmailHandlerOptions>();

            blueprintApiConfigurer.Services.AddTransient<INotificationRepository, EmbeddedResourceNotificationRepository>();
            blueprintApiConfigurer.Services.AddTransient<INotificationService, NotificationService>();
            blueprintApiConfigurer.Services.AddTransient<INotificationHandler, TemplatedEmailHandler>();
            blueprintApiConfigurer.Services.AddTransient<ITemplateFactory, NVelocityTemplateFactory>();
            blueprintApiConfigurer.Services.AddTransient<INotificationRepository, EmbeddedResourceNotificationRepository>();
            blueprintApiConfigurer.Services.AddTransient<IEmailSender, SmtpClientEmailSender>();

            return blueprintApiConfigurer;
        }
    }
}
