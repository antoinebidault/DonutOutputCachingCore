using DonutOutputCachingCore;
using DonutOutputCachingCore.CacheHoleAttribute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
  /// <summary>
  /// Extension methods to register the output caching service.
  /// </summary>
  public static class ServiceCollectionExtensions
  {
    /// <summary>
    /// Registers the output caching service with the dependency injection system.
    /// </summary>
    public static void AddDonutOutputCaching(this IServiceCollection services)
    {
      var options = new OutputCacheOptions();

      BaseServiceRegistration(services, options);
    }

    /// <summary>
    /// Registers the output caching service with the dependency injection system.
    /// </summary>
    public static void AddDonutOutputCaching(this IServiceCollection services, Action<OutputCacheOptions> outputCacheOptions)
    {
      var options = new OutputCacheOptions();
      outputCacheOptions(options);
      BaseServiceRegistration(services, options);
    }

    /// <summary>
    /// Service registration basic
    /// </summary>
    /// <param name="services"></param>
    private static void BaseServiceRegistration(IServiceCollection services, OutputCacheOptions options)
    {
      services.AddSingleton(options);
      services.AddSingleton<ViewComponentHelperFactory>();
      services.AddTransient<DonutOutputCacheHandler>();
      services.AddTransient<OutputCacheHandler>();
      services.TryAddSingleton<IOutputCachingService, OutputCachingService>();
    }
  }
}