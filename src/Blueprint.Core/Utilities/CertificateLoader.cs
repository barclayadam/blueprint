using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace Blueprint.Core.Utilities
{
    public static class CertificateLoader
    {
        public static X509Certificate2 GetCertificateFromStore(string thumbprint)
        {
            var store = new X509Store(StoreLocation.CurrentUser);

            try
            {
                store.Open(OpenFlags.ReadOnly);

                var certCollection = store.Certificates;
                var currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                var signingCert = currentCerts.Find(X509FindType.FindByThumbprint, thumbprint, false);

                if (signingCert.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Could not find token signing certificate. cert_thumbprint={0}".Fmt(thumbprint));
                }

                return signingCert[0];
            }
            finally
            {
                store.Close();
            }
        }

        public static X509Certificate2 ExtractCertificateFromResource(
                Assembly assembly,
                string resourceName,
                string password)
        {
            return new X509Certificate2(
                ExtractResource(assembly, resourceName),
                password,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
        }

        private static byte[] ExtractResource(Assembly a, string fileName)
        {
            var cert = a.GetEmbeddedResourceAsByteArray(fileName);

            if (cert == null)
            {
                throw new InvalidOperationException($"Cannot find required certificate {fileName}");
            }

            return cert;
        }
    }
}
