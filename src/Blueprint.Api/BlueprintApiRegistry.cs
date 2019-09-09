using System.Diagnostics.CodeAnalysis;
using System.Runtime.Caching;
using Blueprint.Api.Authorisation;
using Blueprint.Api.Formatters;
using Blueprint.Api.Validation;
using Blueprint.Core.Auditing;
using Blueprint.Core.Authorisation;
using Blueprint.Core.Caching;
using Blueprint.Core.Errors;
using Blueprint.Core.Tasks;
using StructureMap;

namespace Blueprint.Api
{
    public class BlueprintApiRegistry : Registry
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "StructureMap registry by definition has high class coupling")]
        public BlueprintApiRegistry()
        {
            Scan(
                 s =>
                 {
                     s.AssemblyContainingType<BlueprintApiNamespace>();
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

            For<MemoryCache>().Use(MemoryCache.Default);
        }
    }
}
