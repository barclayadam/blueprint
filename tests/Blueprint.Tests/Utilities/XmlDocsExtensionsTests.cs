using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Blueprint.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Utilities;

public class XmlDocsExtensionsTests
{
    public class WithComplexXmlDoc
    {
        /// <summary>
        /// Query and manages users.
        ///
        /// Please note:
        /// * Users ...
        /// * Users ...
        ///    * Users ...
        ///    * Users ...
        ///
        /// You need one of the following role: Owner, Editor, use XYZ to manage permissions.
        /// </summary>
        public string Foo { get; set; }
    }

    [Test]
    public void When_xml_doc_with_multiple_breaks_is_read_then_they_are_not_stripped_away()
    {
        // Act
        var summary = typeof(WithComplexXmlDoc).GetProperty("Foo").GetXmlDocsSummary();

        // Assert
        summary.Should().StartWith("Query and manages users");
        summary.Should().Contain("\n            \n");
        summary.Should().Contain("   * Users");
        summary.Trim().Should().Be(summary);
    }

    public class WithTagsInXmlDoc
    {
        /// <summary>Gets or sets the foo.</summary>
        /// <response code="201">Account created</response>
        /// <response code="400">Username already in use</response>
        public string Foo { get; set; }
    }

    [Test]
    public void When_xml_doc_contains_xml_then_it_is_fully_read()
    {
        // Act
        var element = typeof(WithTagsInXmlDoc).GetProperty("Foo").GetXmlDocsElement();
        var responses = element.Elements("response");

        // Assert
        responses.Count().Should().Be(2);

        responses.First().Value.Should().Be("Account created");
        responses.First().Attribute("code").Value.Should().Be("201");

        responses.Last().Value.Should().Be("Username already in use");
        responses.Last().Attribute("code").Value.Should().Be("400");
    }

    public class WithSeeTagInXmlDoc
    {
        /// <summary><see langword="null"/> for the default <see cref="WithSeeTagInXmlDoc"/>. See <see cref="WithSeeTagInXmlDoc">this</see> and <see href="https://github.com/rsuter/njsonschema">this</see> at <see href="https://github.com/rsuter/njsonschema"/>.</summary>
        public string Foo { get; set; }
    }

    [Test]
    public void When_summary_has_see_tag_then_it_is_converted()
    {
        // Act
        var summary = typeof(WithSeeTagInXmlDoc).GetProperty("Foo").GetXmlDocsSummary();

        // Assert
        summary.Should().Be("null for the default WithSeeTagInXmlDoc. See this and this at https://github.com/rsuter/njsonschema.");
    }

    public class WithParamrefTagInXmlDoc
    {
        /// <summary>Returns <paramref name="name"/>.</summary>
        /// <param name="name">The name to return.</param>
        public string Foo(string name) => name;
    }

    [Test]
    public void When_summary_has_paramref_tag_then_it_is_converted()
    {
        // Act
        var summary = typeof(WithParamrefTagInXmlDoc).GetMethod("Foo").GetXmlDocsSummary();

        // Assert
        summary.Should().Be("Returns name.");
    }

    public class WithGenericTagsInXmlDoc
    {
        /// <summary>These <c>are</c> <strong>some</strong> tags.</summary>
        public string Foo { get; set; }
    }

    [Test]
    public void When_summary_has_generic_tags_then_it_is_converted()
    {
        // Act
        var summary = typeof(WithGenericTagsInXmlDoc).GetProperty("Foo").GetXmlDocsSummary();

        // Assert
        summary.Should().Be("These are some tags.");
    }

    [Test]
    public void When_xml_doc_is_missing_then_summary_is_missing()
    {
        // Act
        var summary = typeof(Point).GetXmlDocsSummary();

        // Assert
        summary.Should().BeEmpty();
    }

    public abstract class BaseBaseClass
    {
        /// <summary>Foo.</summary>
        public abstract string Foo { get; }

        /// <summary>Bar.</summary>
        /// <param name="baz">Baz.</param>
        public abstract void Bar(string baz);
    }

    public abstract class BaseClass : BaseBaseClass
    {
        /// <inheritdoc />
        public override string Foo { get; }

        /// <inheritdoc />
        public override void Bar(string baz) { }
    }

    public class ClassWithInheritdoc : BaseClass
    {
        /// <inheritdoc />
        public override string Foo { get; }

        /// <inheritdoc />
        public override void Bar(string baz) { }
    }

    [Test]
    public void When_parameter_has_inheritdoc_then_it_is_resolved()
    {
        // Act
        var parameterXml = typeof(ClassWithInheritdoc).GetMethod("Bar").GetParameters()
            .Single(p => p.Name == "baz").GetXmlDocs();

        // Assert
        parameterXml.Should().Be("Baz.");
    }

    [Test]
    public void When_property_has_inheritdoc_then_it_is_resolved()
    {
        // Act
        var propertySummary = typeof(ClassWithInheritdoc).GetProperty("Foo").GetXmlDocsSummary();

        // Assert
        propertySummary.Should().Be("Foo.");
    }

    [Test]
    public void When_method_has_inheritdoc_then_it_is_resolved()
    {
        // Act
        var methodSummary = typeof(ClassWithInheritdoc).GetMethod("Bar").GetXmlDocsSummary();

        // Assert
        methodSummary.Should().Be("Bar.");
    }

    public interface IBaseBaseInterface
    {
        /// <summary>Foo.</summary>
        string Foo { get; }

        /// <summary>Bar.</summary>
        /// <param name="baz">Baz.</param>
        void Bar(string baz);
    }

    public interface IBaseInterface : IBaseBaseInterface
    {
    }

    public class ClassWithInheritdocOnInterface : IBaseInterface
    {
        /// <inheritdoc />
        public string Foo { get; }

        /// <inheritdoc />
        public void Bar(string baz) { }
    }

    [Test]
    public void When_parameter_has_inheritdoc_on_interface_then_it_is_resolved()
    {
        // Act
        var parameterXml = typeof(ClassWithInheritdocOnInterface).GetMethod("Bar").GetParameters()
            .Single(p => p.Name == "baz").GetXmlDocs();

        // Assert
        parameterXml.Should().Be("Baz.");
    }

    [Test]
    public void When_property_has_inheritdoc_on_interface_then_it_is_resolved()
    {
        // Act
        var propertySummary = typeof(ClassWithInheritdocOnInterface).GetProperty("Foo").GetXmlDocsSummary();

        // Assert
        propertySummary.Should().Be("Foo.");
    }

    [Test]
    public void When_method_has_inheritdoc_then_on_interface_it_is_resolved()
    {
        // Act
        var methodSummary = typeof(ClassWithInheritdocOnInterface).GetMethod("Bar").GetXmlDocsSummary();

        // Assert
        methodSummary.Should().Be("Bar.");
    }

    public abstract class MyBaseClass
    {
        /// <summary>
        /// Foo
        /// </summary>
        public void Foo(int p)
        {
        }
    }

    public class MyClass : MyBaseClass
    {
        /// <summary>
        /// Bar
        /// </summary>
        public void Bar()
        {
        }
    }

    [Test]
    public void When_method_is_inherited_then_xml_docs_are_correct()
    {
        // Act
        var fooSummary = typeof(MyClass).GetMethod(nameof(MyClass.Foo)).GetXmlDocsSummary();
        var barSummary = typeof(MyClass).GetMethod(nameof(MyClass.Bar)).GetXmlDocsSummary();

        // Assert
        fooSummary.Should().Be("Foo");
        barSummary.Should().Be("Bar");
    }

    public abstract class BaseController<T>
    {
        /// <summary>Base method.</summary>
        public string Test()
        {
            return null;
        }
    }

    public class MyController : BaseController<string>
    {
    }

    [Test]
    public void WhenTypeInheritsFromGenericType_ThenXmlDocsIsFound()
    {
        // Act
        var fooSummary = typeof(MyController).GetMethod(nameof(BaseController<string>.Test)).GetXmlDocsSummary();

        // Assert
        fooSummary.Should().Be("Base method.");
    }

    public class BaseGenericClass<T1, T2>
    {
        /// <summary>
        /// SingleAsync
        /// </summary>
        public Task<T1> SingleAsync(T2 foo, T1 bar)
        {
            throw new NotImplementedException();
        }

        /// <summary>Baz</summary>
        public T2 Baz { get; set; }
    }

    public class InheritedGenericClass : BaseGenericClass<string, int>
    {
    }

    [Test]
    public void WhenTypeInheritsFromGenericType_ThenMethodAndPropertyWithGenericParametersResolvesCorrectXml()
    {
        // Act
        var summaryMethod = typeof(InheritedGenericClass).GetMethod("SingleAsync").GetXmlDocsSummary();
        var summaryProperty = typeof(InheritedGenericClass).GetProperty("Baz").GetXmlDocsSummary();

        // Assert
        summaryMethod.Should().Be("SingleAsync");
        summaryProperty.Should().Be("Baz");
    }

    public class BaseGenericClass<T>
    {
        /// <summary>
        /// Single
        /// </summary>
        public T Single(T input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Multi
        /// </summary>
        public ICollection<T> Multi(ICollection<T> input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// MultiGenericParameter
        /// </summary>
        public IDictionary<string, string> MultiGenericParameter(IDictionary<string, string> input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// NestedGenericParameter
        /// </summary>
        public IDictionary<string, IDictionary<string, IDictionary<string, string>>> NestedGenericParameter(IDictionary<string, IDictionary<string, IDictionary<string, string>>> input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// SingleAsync
        /// </summary>
        public Task<T> SingleAsync(T input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// MultiAsync
        /// </summary>
        public Task<ICollection<T>> MultiAsync(ICollection<T> input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// MultiGenericParameterAsync
        /// </summary>
        public Task<IDictionary<string, string>> MultiGenericParameterAsync(IDictionary<string, string> input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// NestedGenericParameterAsync
        /// </summary>
        public Task<IDictionary<string, IDictionary<string, IDictionary<string, string>>>> NestedGenericParameterAsync(IDictionary<string, IDictionary<string, IDictionary<string, string>>> input)
        {
            throw new NotImplementedException();
        }

    }

    public class InheritedGenericClass2 : BaseGenericClass<string>
    {
    }

    [Test]
    public void When_method_is_inherited_from_generic_class_then_xml_docs_are_correct()
    {
        // Act
        var singleSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.Single)).GetXmlDocsSummary();
        var multiSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.Multi)).GetXmlDocsSummary();
        var multiGenericParameterSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.MultiGenericParameter)).GetXmlDocsSummary();
        var nestedGenericParameterSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.NestedGenericParameter)).GetXmlDocsSummary();
        var singleAsyncSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.SingleAsync)).GetXmlDocsSummary();
        var multiAsyncSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.MultiAsync)).GetXmlDocsSummary();
        var multiGenericParameterAsyncSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.MultiGenericParameterAsync)).GetXmlDocsSummary();
        var nestedGenericParameterAsyncSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.NestedGenericParameterAsync)).GetXmlDocsSummary();

        // Assert
        singleSummary.Should().Be("Single");
        multiSummary.Should().Be("Multi");
        multiGenericParameterSummary.Should().Be("MultiGenericParameter");
        nestedGenericParameterSummary.Should().Be("NestedGenericParameter");
        singleAsyncSummary.Should().Be("SingleAsync");
        multiAsyncSummary.Should().Be("MultiAsync");
        multiGenericParameterAsyncSummary.Should().Be("MultiGenericParameterAsync");
        nestedGenericParameterAsyncSummary.Should().Be("NestedGenericParameterAsync");
    }

    public class BusinessProcessSearchResult : SearchBehaviorBaseResult<BusinessProcess>
    {
    }

    public class BusinessProcess
    {
    }

    public class SearchBehaviorBaseResult<T> : BaseResult<T>, ISearchBehaviorResult
    {
        /// <inheritdoc />
        public string SearchString { get; set; }

        /// <inheritdoc />
        public bool IsSearchStringRewritten { get; set; }
    }

    public interface ISearchBehaviorResult
    {
        /// <summary>
        /// The search string used to query the data
        /// </summary>
        string SearchString { get; set; }

        /// <summary>
        /// Flag to notify if the SearchString was modified compared to the original requested one
        /// </summary>
        bool IsSearchStringRewritten { get; set; }
    }

    /// <summary>
    /// Base class for search results
    /// </summary>
    /// <typeparam name="T">Type of the results</typeparam>
    public class BaseResult<T> : IPagedSearchResult
    {
        /// <inheritdoc />
        public string PageToken { get; set; }
    }

    public interface IPagedSearchResult
    {
        /// <summary>
        /// An optional token to access the next page of results for those endpoints that support a backend scrolling logic.
        /// </summary>
        string PageToken { get; set; }
    }

    [Test]
    public void When_inheritdocs_is_availble_in_inheritance_chain_then_it_is_resolved()
    {
        // Act
        var searchStringProperty = typeof(BusinessProcessSearchResult).GetRuntimeProperty("SearchString").GetXmlDocsSummary();
        var isSearchStringRewrittenProperty = typeof(BusinessProcessSearchResult).GetRuntimeProperty("IsSearchStringRewritten").GetXmlDocsSummary();
        var pageTokenProperty = typeof(BusinessProcessSearchResult).GetRuntimeProperty("PageToken").GetXmlDocsSummary();

        // Assert
        Assert.True(!string.IsNullOrWhiteSpace(searchStringProperty));
        Assert.True(!string.IsNullOrWhiteSpace(isSearchStringRewrittenProperty));
        Assert.True(!string.IsNullOrWhiteSpace(pageTokenProperty));
    }

    /// <summary>
    /// The publisher.
    /// </summary>
    public class Publisher
    {
        /// <summary>
        /// The name of the publisher.
        /// </summary>
        public string Name { get; set; }
    }

    [Test]
    public void When_type_has_summary_then_it_is_read()
    {
        // Act
        var summary = typeof(Publisher).GetXmlDocsSummary();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(summary));
    }
}