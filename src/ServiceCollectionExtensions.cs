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

      services.AddSingleton(options);
      services.AddSingleton<ViewComponentHelperFactory>();
      services.AddSingleton<DonutOutputCacheHandler>();
      services.AddSingleton<IOutputCachingService, OutputCachingService>();
    }

    /// <summary>
    /// Registers the output caching service with the dependency injection system.
    /// </summary>
    public static void AddDonutOutputCaching(this IServiceCollection services, Action<OutputCacheOptions> outputCacheOptions)
    {
      var options = new OutputCacheOptions();
      outputCacheOptions(options);

      services.AddSingleton(options);
      services.AddSingleton<ViewComponentHelperFactory>();
      services.AddSingleton<DonutOutputCacheHandler>();
      services.TryAddSingleton<IOutputCachingService, OutputCachingService>();
    }
  }
}