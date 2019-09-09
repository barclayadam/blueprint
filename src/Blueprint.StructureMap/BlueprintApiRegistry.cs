using System.Diagnostics.CodeAnalysis;
using System.Runtime.Caching;
using Blueprint.Api;
using Blueprint.Api.Authorisation;
using Blueprint.Api.Formatters;
using Blueprint.Api.Validation;
using Blueprint.StructureMap.CodeGen;
using StructureMap;

namespace Blueprint.StructureMap
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

                     s.AddAllTypesOf<ITypeFormatter>();

                     s.AddAllTypesOf<IResourceLinkGenerator>();

                     s.AddAllTypesOf<IApiAuthoriser>();
                     
                     s.AddAllTypesOf<IValidationSource>();
                     s.AddAllTypesOf<IValidationSourceBuilder>();

                     s.ConnectImplementationsToTypesClosing(typeof(IApiOperationHandler<>));
                 });

            For<IValidator>().Use<BlueprintValidator>();
            For<IInstanceFrameProvider>().Use<StructureMapInstanceFrameProvider>();

            For<MemoryCache>().Use(MemoryCache.Default);
        }
    }
}
