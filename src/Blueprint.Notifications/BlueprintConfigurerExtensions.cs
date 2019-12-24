using System.Reflection;
using Blueprint.Api.Configuration;
using Blueprint.Notifications.Handlers;
using Blueprint.Notifications.Templates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blueprint.Notifications
{
    public static class BlueprintConfigurerExtensions
    {
        public static BlueprintApiConfigurer AddNotifications(this BlueprintApiConfigurer blueprintApiConfigurer, params Assembly[] embeddedResourceAssemblies)
        {
            if (embeddedResourceAssemblies.Length == 0)
            {
                embeddedResourceAssemblies = new[] {Assembly.GetEntryAssembly()};
            }

            blueprintApiConfigurer.Services.AddOptions<TemplatedEmailHandlerOptions>();

            blueprintApiConfigurer.Services.AddTransient<INotificationRepository>(p =>
                new EmbeddedResourceNotificationRepository(embeddedResourceAssemblies, p.GetRequiredService<ILogger<EmbeddedResourceNotificationRepository>>()));

            blueprintApiConfigurer.Services.AddTransient<INotificationService, NotificationService>();
            blueprintApiConfigurer.Services.AddTransient<INotificationHandler, TemplatedEmailHandler>();
            blueprintApiConfigurer.Services.AddTransient<ITemplateFactory, NVelocityTemplateFactory>();
            blueprintApiConfigurer.Services.AddTransient<IEmailSender, SmtpClientEmailSender>();

            return blueprintApiConfigurer;
        }
    }
}
