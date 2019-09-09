using System.Diagnostics.CodeAnalysis;
using System.Runtime.Caching;

using Blueprint.Core.Api;
using Blueprint.Core.Api.Authorisation;
using Blueprint.Core.Api.Formatters;
using Blueprint.Core.Auditing;
using Blueprint.Core.Authorisation;
using Blueprint.Core.Caching;
using Blueprint.Core.Errors;
using Blueprint.Core.Notifications;
using Blueprint.Core.Notifications.Handlers;
using Blueprint.Core.Tasks;
using Blueprint.Core.Templates;
using Blueprint.Core.Validation;

using StructureMap;

namespace Blueprint.Core
{
    public class BlueprintRegistry : Registry
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "StructureMap registry by definition has high class coupling")]
        public BlueprintRegistry()
        {
            Scan(
                 s =>
                 {
                     s.AssemblyContainingType<BlueprintCoreNamespace>();
                     s.WithDefaultConventions();

                     s.AddAllTypesOf<IExceptionSink>();
                     s.AddAllTypesOf<IExceptionFilter>();

                     s.AddAllTypesOf<ITypeFormatter>();

                     s.AddAllTypesOf<IResourceKeyExpander>();
                     s.AddAllTypesOf<IResourceLinkGenerator>();

                     s.AddAllTypesOf<ITaskScheduler>();

                     s.AddAllTypesOf<IApiAuthoriser>();

                     s.AddAllTypesOf<IExceptionSink>();
                     s.AddAllTypesOf<IExceptionFilter>();

                     s.AddAllTypesOf<IValidationSource>();
                     s.AddAllTypesOf<IValidationSourceBuilder>();

                     s.ConnectImplementationsToTypesClosing(typeof(IApiOperationHandler<>));
                 });

            For<ICache>().Use<Cache>().Singleton();

            For<IErrorLogger>().Use<ErrorLogger>().Singleton();

            For<IValidator>().Use<BlueprintValidator>();

            For<IAuditor>().Use<Auditor>();

            For<INotificationRepository>().Use<EmbeddedResourceNotificationRepository>();

            For<INotificationService>().Use<NotificationService>();
            For<INotificationHandler>().Use<TemplatedEmailHandler>();
            For<ITemplateFactory>().Use<NVelocityTemplateFactory>();
            For<IEmailSender>().Use<SmtpClientEmailSender>();

            For<MemoryCache>().Use(MemoryCache.Default);
        }
    }
}
