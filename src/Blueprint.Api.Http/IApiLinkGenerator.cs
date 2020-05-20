using System;
using Blueprint.Api.Configuration;

namespace Blueprint.Api.Http
{
    public interface IApiLinkGenerator
    {
        /// <summary>
        /// Creates the "self" link for the resource type represented by <typeparamref name="T" />, expecting the
        /// URL to contain a single placeholder, <c>id</c>, that will be given the value of <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the resource being linked to.</param>
        /// <param name="queryString">An optional object that contains key-value pair of values to add to the query string of the generated link.</param>
        /// <typeparam name="T">The <see cref="ApiResource"/> type.</typeparam>
        /// <returns>A new <see cref="Link"/> that references the given resource.</returns>
        Link CreateSelfLink<T>(int id, object queryString = null) where T : ApiResource;

        /// <summary>
        /// Creates the "self" link for the resource type represented by <typeparamref name="T" />, expecting the
        /// URL to contain a single placeholder, <c>id</c>, that will be given the value of <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the resource being linked to.</param>
        /// <param name="queryString">An optional object that contains key-value pair of values to add to the query string of the generated link.</param>
        /// <typeparam name="T">The <see cref="ApiResource"/> type.</typeparam>
        /// <returns>A new <see cref="Link"/> that references the given resource.</returns>
        Link CreateSelfLink<T>(long id, object queryString = null) where T : ApiResource;

        /// <summary>
        /// Creates the "self" link for the resource type represented by <typeparamref name="T" />, expecting the
        /// URL to contain a single placeholder, <c>id</c>, that will be given the value of <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the resource being linked to.</param>
        /// <param name="queryString">An optional object that contains key-value pair of values to add to the query string of the generated link.</param>
        /// <typeparam name="T">The <see cref="ApiResource"/> type.</typeparam>
        /// <returns>A new <see cref="Link"/> that references the given resource.</returns>
        Link CreateSelfLink<T>(string id, object queryString = null) where T : ApiResource;

        /// <summary>
        /// Creates the "self" link for the resource type represented by <typeparamref name="T" />, expecting the
        /// URL to contain a single placeholder, <c>id</c>, that will be given the value of <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the resource being linked to.</param>
        /// <param name="queryString">An optional object that contains key-value pair of values to add to the query string of the generated link.</param>
        /// <typeparam name="T">The <see cref="ApiResource"/> type.</typeparam>
        /// <returns>A new <see cref="Link"/> that references the given resource.</returns>
        Link CreateSelfLink<T>(Guid id, object queryString = null) where T : ApiResource;

        /// <summary>
        /// Creates the "self" link for the resource type represented by <typeparamref name="T" />, filling in
        /// the URL placeholders with values from the <paramref name="idDefinition" /> parameter.
        /// </summary>
        /// <param name="idDefinition">An object that contains properties used to fill the link (typically the ApiResource represented by the links'
        /// <see cref="ApiOperationLink.ResourceType"/> property specified as <typeparamref name="T" />).
        /// </param>
        /// <param name="queryString">An optional object that contains key-value pair of values to add to the query string of the generated link.</param>
        /// <typeparam name="T">The resource type.</typeparam>
        /// <returns>A Link representing 'self' for the given resource type.</returns>
        Link CreateSelfLink<T>(object idDefinition, object queryString = null) where T : ApiResource;

        /// <summary>
        /// Creates a fully qualified URL (using <see cref="BlueprintApiOptions.BaseApiUrl" />) for the specified link
        /// and "result" object that is used to fill the placeholders of the link.
        /// </summary>
        /// <param name="link">The link to generate URL for.</param>
        /// <param name="result">The "result" object used to populate placeholder values of the specified link route.</param>
        /// <returns>A fully-qualified URL.</returns>
        string CreateUrl(ApiOperationLink link, object result = null);

        /// <summary>
        /// Given a populated <see cref="IApiOperation" /> will generate a fully-qualified URL that, when hit, would execute the operation
        /// with the specified values.
        /// </summary>
        /// <remarks>
        /// This will use the <em>first</em> link (route) specified for the operation.
        /// </remarks>
        /// <param name="operation">The operation to generate a URL for.</param>
        /// <returns>A fully-qualified URL that, if hit, would execute the passed in operation with the same values.</returns>
        /// <exception cref="InvalidOperationException">If no links / routes have been specified for the given operation.</exception>
        string CreateUrl(IApiOperation operation);
    }
}
