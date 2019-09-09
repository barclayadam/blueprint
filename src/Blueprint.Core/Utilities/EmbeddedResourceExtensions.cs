using System;
using System.IO;
using System.Reflection;

namespace Blueprint.Core.Utilities
{
    public static class EmbeddedResourceExtensions
    {
        public static string GetRelativeResourceName(this Type type, string fileName)
        {
            return type.Namespace + "." + fileName;
        }

        public static string GetEmbeddedResourceAsString(this Assembly assembly, string name)
        {
            using (var stream = assembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static string GetEmbeddedResourceAsString(this Type type, string name)
        {
            return GetEmbeddedResourceAsString(type.Assembly, name);
        }

        public static string GetRelativeEmbeddedResourceAsString(this Type type, string name)
        {
            return GetEmbeddedResourceAsString(type.Assembly, type.GetRelativeResourceName(name));
        }

        public static byte[] GetEmbeddedResourceAsByteArray(this Assembly assembly, string name)
        {
            using (var resFilestream = assembly.GetManifestResourceStream(name))
            {
                if (resFilestream == null)
                {
                    return null;
                }

                var ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);

                return ba;
            }
        }

        public static byte[] GetEmbeddedResourceAsByteArray(this Type type, string name)
        {
            return GetEmbeddedResourceAsByteArray(type.Assembly, name);
        }

        public static byte[] GetRelativeEmbeddedResourceAsByteArray(this Type type, string name)
        {
            return GetEmbeddedResourceAsByteArray(type.Assembly, type.GetRelativeResourceName(name));
        }
    }
}