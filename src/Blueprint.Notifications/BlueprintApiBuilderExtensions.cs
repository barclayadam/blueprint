using System.Reflection;
using Blueprint.Notifications;
using Blueprint.Notifications.Handlers;
using Blueprint.Notifications.Templates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace For discoverability we add to existing namespace
namespace Blueprint.Configuration
{
    public static class BlueprintApiBuilderExtensions
    {
        public static BlueprintApiBuilder<THost> AddNotifications<THost>(this BlueprintApiBuilder<THost> blueprintApiBuilder, params Assembly[] embeddedResourceAssemblies)
        {
            if (embeddedResourceAssemblies.Length == 0)
            {
                embeddedResourceAssemblies = new[] {Assembly.GetEntryAssembly()};
            }

            blueprintApiBuilder.Services.AddOptions<TemplatedEmailHandlerOptions>();

            blueprintApiBuilder.Services.AddTransient<INotificationRepository>(p =>
                new EmbeddedResourceNotificationRepository(embeddedResourceAssemblies, p.GetRequiredService<ILogger<EmbeddedResourceNotificationRepository>>()));

            blueprintApiBuilder.Services.AddTransient<INotificationService, NotificationService>();
            blueprintApiBuilder.Services.AddTransient<INotificationHandler, TemplatedEmailHandler>();
            blueprintApiBuilder.Services.AddTransient<ITemplateFactory, NVelocityTemplateFactory>();
            blueprintApiBuilder.Services.AddTransient<IEmailSender, SmtpClientEmailSender>();

            return blueprintApiBuilder;
        }
    }
}
