using System;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Blueprint.Utilities
{
    public static class CertificateManager
    {
        private static readonly char[] SettingsSeparator = { ';' };

        public static X509Certificate2 Load(string settingsValue)
        {
            var arguments = settingsValue.Split(SettingsSeparator);
            var loadType = arguments[0];
            var loadArguments = arguments.Skip(1).ToArray();

            if (loadType == "Thumbprint")
            {
                return LoadByThumbprint(loadArguments);
            }

            if (loadType == "Resource")
            {
                return LoadByResource(loadArguments);
            }

            throw new ConfigurationErrorsException(
                $"Certificate setting type {loadType} is not recognised.");
        }

        private static X509Certificate2 LoadByResource(string[] arguments)
        {
            if (arguments.Length != 3)
            {
                throw new ConfigurationErrorsException(
                    "Certificate loading by resource requires 3 arguments, assembly name, resource name + password");
            }

            var assemblyName = arguments[0];
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.FullName.StartsWith(assemblyName));
            var resourceName = assemblyName + "." + arguments[1];
            var password = arguments[2];

            if (assembly == null)
            {
                throw new ConfigurationErrorsException(
                    $"Certificate loading by resource failed because cannot find assembly {assemblyName}");
            }

            return CertificateLoader.ExtractCertificateFromResource(
                assembly,
                resourceName,
                password);
        }

        private static X509Certificate2 LoadByThumbprint(string[] arguments)
        {
            if (arguments.Length != 1)
            {
                throw new ConfigurationErrorsException(
                    "Certificate loading by thumbprint requires 1 argument, the thumbprint");
            }

            return CertificateLoader.GetCertificateFromStore(arguments[0]);
        }
    }
}
