using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;
using DonutOutputCachingCore;
using DonutOutputCachingCore.CacheHoleAttribute;

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
        public static void AddOutputCaching(this IServiceCollection services)
        {
            var options = new OutputCacheOptions();

            services.AddSingleton(options);
            //   services.AddTransient<IViewBufferScope, MemoryPoolViewBufferScope>();
            //services.AddTransient<IViewComponentHelper, DefaultViewComponentHelper>();
            services.AddSingleton<ViewComponentHelperFactory>();
            services.AddTransient<DonutOutputCacheHandler>();
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
            services.AddTransient<IViewBufferScope, MemoryPoolViewBufferScope>();
            services.AddTransient<IViewComponentHelper, DefaultViewComponentHelper>();
            services.AddTransient<DonutOutputCacheHandler>();
            services.TryAddSingleton<IOutputCachingService, OutputCachingService>();
        }
    }
}