﻿using System;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Tasks.Hangfire;

/// <summary>
/// A Hangfire <see cref="JobActivator"/> that uses Microsoft's DI <see cref="IServiceProvider"/> abstraction.
/// </summary>
public class ServiceProviderJobActivator : JobActivator
{
    private readonly IServiceProvider _rootServiceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceProviderJobActivator"/>
    /// class with a given <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="rootServiceProvider">Container that will be used to create instances of classes during the job activation process.</param>
    public ServiceProviderJobActivator(IServiceProvider rootServiceProvider)
    {
        this._rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException(nameof(rootServiceProvider));
    }

    /// <inheritdoc />
    public override object ActivateJob(Type jobType)
    {
        return this._rootServiceProvider.GetRequiredService(jobType);
    }

    /// <inheritdoc />
    [Obsolete("Please implement/use the BeginScope(JobActivatorContext) method instead. Will be removed in 2.0.0.")]
    public override JobActivatorScope BeginScope()
    {
        return new ServiceProviderDependencyScope(this._rootServiceProvider.CreateScope());
    }

    /// <inheritdoc />
    public override JobActivatorScope BeginScope(JobActivatorContext context)
    {
        return new ServiceProviderDependencyScope(this._rootServiceProvider.CreateScope());
    }

    private class ServiceProviderDependencyScope : JobActivatorScope
    {
        private readonly IServiceScope _nestedContainer;

        public ServiceProviderDependencyScope(IServiceScope nestedContainer)
        {
            this._nestedContainer = nestedContainer;
        }

        public override object Resolve(Type type)
        {
            return this._nestedContainer.ServiceProvider.GetRequiredService(type);
        }

        public override void DisposeScope()
        {
            this._nestedContainer.Dispose();
        }
    }
}