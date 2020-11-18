// Adapted from code in Namotion.Reflection (https://github.com/RicoSuter/Namotion.Reflection/blob/master/src/Namotion.Reflection/XmlDocsExtensions.cs at 18/11/2020).
// Modified here to remove lock and use of dynamic for CPU and memory reduction

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Blueprint.Utilities
{
    /// <summary>
    /// Provides extension methods for reading XML comments from reflected members, utilising the XML documentation file
    /// that is generated when &lt;GenerateDocumentationFile&gt; is <c>true</c>.
    /// </summary>
    public static class XmlDocsExtensions
    {
        private static readonly Dictionary<string, XDocument> Cache = new Dictionary<string, XDocument>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsSummary(this Type type)
        {
            return GetXmlDocsTag((MemberInfo)type.GetTypeInfo(), "summary");
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsRemarks(this Type type)
        {
            return GetXmlDocsTag((MemberInfo)type.GetTypeInfo(), "remarks");
        }

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsSummary(this MemberInfo member)
        {
            return GetXmlDocsTag(member, "summary");
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsRemarks(this MemberInfo member)
        {
            return GetXmlDocsTag(member, "remarks");
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsTag(this Type type, string tagName)
        {
            return GetXmlDocsTag((MemberInfo)type.GetTypeInfo(), tagName);
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static XElement GetXmlDocsElement(this MemberInfo member)
        {
            return GetXmlElement(member);
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsTag(this MemberInfo member, string tagName)
        {
            _ = member ?? throw new ArgumentNullException(nameof(member));
            _ = tagName ?? throw new ArgumentNullException(nameof(tagName));

            var assemblyName = member.Module.Assembly.GetName();

            if (IsAssemblyMissingDocumentationInCache(assemblyName))
            {
                return string.Empty;
            }

            var documentationPath = GetXmlDocsPath(member.Module.Assembly);
            var element = GetXmlElement(member, documentationPath);
            return ToXmlDocsContent(element?.Element(tagName));
        }

        /// <summary>Returns the contents of the "returns" or "param" XML documentation tag for the specified parameter.</summary>
        /// <param name="parameter">The reflected parameter or return info.</param>
        /// <returns>The contents of the "returns" or "param" tag.</returns>
        public static string GetXmlDocs(this ParameterInfo parameter)
        {
            var assemblyName = parameter.Member.Module.Assembly.GetName();

            if (IsAssemblyMissingDocumentationInCache(assemblyName))
            {
                return string.Empty;
            }

            var documentationPath = GetXmlDocsPath(parameter.Member.Module.Assembly);
            var element = GetXmlElement(parameter, documentationPath);
            return ToXmlDocsContent(element);
        }

        /// <summary>Converts the given XML documentation <see cref="XElement"/> to text.</summary>
        /// <param name="element">The XML element.</param>
        /// <returns>The text.</returns>
        private static string ToXmlDocsContent(this XElement element)
        {
            if (element == null)
            {
                return string.Empty;
            }

            var value = new StringBuilder();

            foreach (var node in element.Nodes())
            {
                if (node is XElement e)
                {
                    if (e.Name == "see")
                    {
                        var attribute = e.Attribute("langword");
                        if (attribute != null)
                        {
                            value.Append(attribute.Value);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(e.Value))
                            {
                                value.Append(e.Value);
                            }
                            else
                            {
                                attribute = e.Attribute("cref");
                                if (attribute != null)
                                {
                                    value.Append(attribute.Value.Trim('!', ':').Trim().Split('.').Last());
                                }
                                else
                                {
                                    attribute = e.Attribute("href");
                                    if (attribute != null)
                                    {
                                        value.Append(attribute.Value);
                                    }
                                }
                            }
                        }
                    }
                    else if (e.Name == "paramref")
                    {
                        var nameAttribute = e.Attribute("name");
                        value.Append(nameAttribute?.Value ?? e.Value);
                    }
                    else
                    {
                        value.Append(e.Value);
                    }
                }
                else
                {
                    value.Append(node);
                }
            }

            return RemoveLineBreakWhiteSpaces(value.ToString());
        }

        private static XElement GetXmlElement(this MemberInfo member)
        {
            var assemblyName = member.Module.Assembly.GetName();
            if (IsAssemblyMissingDocumentationInCache(assemblyName))
            {
                return null;
            }

            var documentationPath = GetXmlDocsPath(member.Module.Assembly);
            return GetXmlElement(member, documentationPath);
        }

        private static XElement GetXmlElement(this ParameterInfo parameter, string pathToXmlFile)
        {
            try
            {
                var assemblyName = parameter.Member.Module.Assembly.GetName();
                var document = TryGetXmlDocsDocument(assemblyName, pathToXmlFile);
                if (document == null)
                {
                    return null;
                }

                return GetXmlDocsElement(parameter, document);
            }
            catch
            {
                return null;
            }
        }

        private static XElement GetXmlElement(this MemberInfo member, string pathToXmlFile)
        {
            try
            {
                var assemblyName = member.Module.Assembly.GetName();
                var document = TryGetXmlDocsDocument(assemblyName, pathToXmlFile);
                if (document == null)
                {
                    return null;
                }

                var element = GetXmlDocsElement(member, document);
                ReplaceInheritdocElements(member, element);
                return element;
            }
            catch
            {
                return null;
            }
        }

        private static XDocument TryGetXmlDocsDocument(AssemblyName assemblyName, string pathToXmlFile)
        {
            if (!Cache.ContainsKey(assemblyName.FullName))
            {
                if (File.Exists(pathToXmlFile) == false)
                {
                    Cache[assemblyName.FullName] = null;
                    return null;
                }

                Cache[assemblyName.FullName] = XDocument.Load(pathToXmlFile, LoadOptions.PreserveWhitespace);
            }

            return Cache[assemblyName.FullName];
        }

        private static bool IsAssemblyMissingDocumentationInCache(AssemblyName assemblyName)
        {
            if (Cache.ContainsKey(assemblyName.FullName) && Cache[assemblyName.FullName] == null)
            {
                return true;
            }

            return false;
        }

        private static XElement GetXmlDocsElement(this MemberInfo member, XDocument xml)
        {
            var name = GetMemberElementName(member);
            return GetXmlDocsElement(xml, name);
        }

        private static XElement GetXmlDocsElement(this XDocument xml, string name)
        {
            var result = (IEnumerable)xml.XPathEvaluate($"/doc/members/member[@name='{name}']");
            return result.OfType<XElement>().FirstOrDefault();
        }

        private static XElement GetXmlDocsElement(this ParameterInfo parameter, XDocument xml)
        {
            var name = GetMemberElementName(parameter.Member);
            var result = (IEnumerable)xml.XPathEvaluate($"/doc/members/member[@name='{name}']");

            var element = result.OfType<XElement>().FirstOrDefault();
            if (element != null)
            {
                ReplaceInheritdocElements(parameter.Member, element);

                if (parameter.IsRetval || string.IsNullOrEmpty(parameter.Name))
                {
                    result = (IEnumerable)xml.XPathEvaluate($"/doc/members/member[@name='{name}']/returns");
                }
                else
                {
                    result = (IEnumerable)xml.XPathEvaluate($"/doc/members/member[@name='{name}']/param[@name='{parameter.Name}']");
                }

                return result.OfType<XElement>().FirstOrDefault();
            }

            return null;
        }

        private static void ReplaceInheritdocElements(this MemberInfo member, XElement element)
        {
            if (element == null)
            {
                return;
            }

            var children = element.Nodes().ToList();
            foreach (var child in children.OfType<XElement>())
            {
                if (child.Name.LocalName.ToLowerInvariant() == "inheritdoc")
                {
                    var baseType = member.DeclaringType.GetTypeInfo().BaseType;
                    var baseMember = baseType?.GetTypeInfo().DeclaredMembers.SingleOrDefault(m => m.Name == member.Name);
                    if (baseMember != null)
                    {
                        var baseDoc = baseMember.GetXmlElement();
                        if (baseDoc != null)
                        {
                            var nodes = baseDoc.Nodes().OfType<object>().ToArray();
                            child.ReplaceWith(nodes);
                        }
                        else
                        {
                            ProcessInheritdocInterfaceElements(member, child);
                        }
                    }
                    else
                    {
                        ProcessInheritdocInterfaceElements(member, child);
                    }
                }
            }
        }

        private static void ProcessInheritdocInterfaceElements(this MemberInfo member, XElement child)
        {
            foreach (var baseInterface in member.DeclaringType.GetTypeInfo().ImplementedInterfaces)
            {
                var baseMember = baseInterface?.GetTypeInfo().DeclaredMembers.SingleOrDefault(m => m.Name == member.Name);
                if (baseMember != null)
                {
                    var baseDoc = baseMember.GetXmlElement();
                    if (baseDoc != null)
                    {
                        var nodes = baseDoc.Nodes().OfType<object>().ToArray();
                        child.ReplaceWith(nodes);
                    }
                }
            }
        }

        // Trims the start of each line in the given documentation string so that they start on the left side:
        //
        // <summary>
        // This is the text
        //
        //    With another indented line
        // <summary>
        //
        // is output as
        //
        // "     This is the text\n     \n        With another indented line"
        //
        // but we want:
        //
        // "This is the text\n\nWith another indented line"
        private static string RemoveLineBreakWhiteSpaces(string documentation)
        {
            if (string.IsNullOrEmpty(documentation))
            {
                return string.Empty;
            }

            documentation = "\n" + documentation.Replace("\r", string.Empty).Trim('\n');

            var i = 1;

            for (; i < documentation.Length; i++)
            {
                if (documentation[i] != ' ')
                {
                    break;
                }
            }

            documentation = documentation.Replace($"\n{new string(' ', i - 1)}", "\n");

            return documentation.Trim('\n');
        }

        /// <exception cref="ArgumentException">Unknown member type.</exception>
        private static string GetMemberElementName(MemberInfo member)
        {
            char prefixCode;

            if (member is MemberInfo memberInfo &&
                memberInfo.DeclaringType != null &&
                memberInfo.DeclaringType.GetTypeInfo().IsGenericType)
            {
                // Resolve member with generic arguments (Ts instead of actual types)
                if (member is PropertyInfo propertyInfo)
                {
                    member = propertyInfo.DeclaringType.GetRuntimeProperty(propertyInfo.Name);
                }
                else
                {
                    member = member.Module.ResolveMember(member.MetadataToken);
                }
            }

            var memberType = member.GetType();

            var memberName = member is Type type && !string.IsNullOrEmpty(memberType.FullName)
                ? type.FullName.Split('[')[0]
                : member.DeclaringType.FullName.Split('[')[0] + "." + member.Name;

            var memberTypeName = member.MemberType.ToString();

            switch (memberTypeName)
            {
                case "Constructor":
                    memberName = memberName.Replace(".ctor", "#ctor");
                    goto case "Method";

                case "Method":
                    prefixCode = 'M';

                    var parameters = member is MethodBase @base
                        ? @base.GetParameters().Select(x =>
                            x.ParameterType.FullName ??
                            (x.ParameterType.GenericTypeArguments.Length > 0
                                ? x.ParameterType.Namespace + "." + x.ParameterType.Name.Split('`')[0] +
                                  "{" + string.Join(
                                      ",",
                                      x.ParameterType.GenericTypeArguments.Select(a => "||" + a.GenericParameterPosition)) + "}"
                                : "||" + x.ParameterType.GenericParameterPosition))
                        : Enumerable.Empty<string>();

                    var paramTypesList = string.Join(",", parameters
                        .Select(x => Regex
                            .Replace(x, "(`[0-9]+)|(, .*?PublicKeyToken=[0-9a-z]*)", string.Empty)
                            .Replace("],[", ",")
                            .Replace("||", "`")
                            .Replace("[[", "{")
                            .Replace("]]", "}"))
                        .ToArray());

                    if (!string.IsNullOrEmpty(paramTypesList))
                    {
                        memberName += "(" + paramTypesList + ")";
                    }

                    break;

                case "Event":
                    prefixCode = 'E';
                    break;

                case "Field":
                    prefixCode = 'F';
                    break;

                case "NestedType":
                    memberName = memberName.Replace('+', '.');
                    goto case "TypeInfo";

                case "TypeInfo":
                    prefixCode = 'T';
                    break;

                case "Property":
                    prefixCode = 'P';
                    break;

                default:
                    throw new ArgumentException("Unknown member type.", nameof(member));
            }

            return $"{prefixCode}:{memberName.Replace("+", ".")}";
        }

        private static string GetXmlDocsPath(Assembly assembly)
        {
            try
            {
                if (assembly == null)
                {
                    return null;
                }

                var assemblyName = assembly.GetName();
                if (string.IsNullOrEmpty(assemblyName.Name))
                {
                    return null;
                }

                if (Cache.ContainsKey(assemblyName.FullName))
                {
                    return null;
                }

                string path;

                if (!string.IsNullOrEmpty(assembly.Location))
                {
                    var assemblyDirectory = Path.GetDirectoryName(assembly.Location);

                    if (assemblyDirectory != null)
                    {
                        path = Path.Combine(assemblyDirectory, assemblyName.Name + ".xml");

                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(assembly.CodeBase))
                {
                    var withoutFileScheme = assembly.CodeBase.Replace("file:///", string.Empty);
                    var directoryName = Path.GetDirectoryName(withoutFileScheme);

                    if (directoryName != null)
                    {
                        path = Path.Combine(directoryName, assemblyName.Name + ".xml");

                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                }

                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                if (!string.IsNullOrEmpty(baseDirectory))
                {
                    path = Path.Combine(baseDirectory, assemblyName.Name + ".xml");
                    if (File.Exists(path))
                    {
                        return path;
                    }

                    return Path.Combine(baseDirectory, "bin\\" + assemblyName.Name + ".xml");
                }

                var currentDirectory = Directory.GetCurrentDirectory();
                path = Path.Combine(currentDirectory, assembly.GetName().Name + ".xml");
                if (File.Exists(path))
                {
                    return path;
                }

                path = Path.Combine(currentDirectory, "bin\\" + assembly.GetName().Name + ".xml");
                if (File.Exists(path))
                {
                    return path;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
