using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;

namespace Blueprint.Http;

/// <summary>
/// Builds conventions that will be used for customization of Blueprint <see cref="EndpointBuilder"/> instances.
/// </summary>
public sealed class BlueprintEndpointRouteBuilder : IEndpointConventionBuilder
{
    private readonly List<IEndpointConventionBuilder> _builders;

    internal BlueprintEndpointRouteBuilder(List<IEndpointConventionBuilder> builders)
    {
        this._builders = builders;
    }

    /// <inheritdoc/>
    public void Add(Action<EndpointBuilder> convention)
    {
        foreach (var b in this._builders)
        {
            b.Add(convention);
        }
    }
}