using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using DonutOutputCachingCore;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extensions for registering the output caching middleware.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Registers the output caching middleware
        /// </summary>
        public static void UseDonutOutputCaching(this IApplicationBuilder app)
        {
            app.UseMiddleware<DonutOutputCacheMiddleware>();
        }
    }
}