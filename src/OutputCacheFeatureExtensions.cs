﻿using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace DonutOutputCachingCore
{
  /// <summary>
  /// Extensions methods for HttpContext
  /// </summary>
  public static class OutputCacheFeatureExtensions
  {
    /// <summary>
    /// Enabled output caching of the response.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="profile">The caching profile to use.</param>
    public static void EnableOutputCaching(this HttpContext context, OutputCacheProfile profile)
    {
      var slidingExpiration = TimeSpan.FromSeconds(profile.Duration);
      string varyByHeader = profile.VaryByHeader;
      string varyByParam = profile.VaryByParam;
      string[] fileDependencies = profile.FileDependencies.ToArray();
      bool useAbsoluteExpiration = profile.UseAbsoluteExpiration;

      context.EnableOutputCaching(slidingExpiration, varyByHeader, varyByParam, useAbsoluteExpiration, fileDependencies);
    }

    public static void InvalidateOutputCaching(this HttpContext context, string url)
    {
      OutputCacheProfile feature = context.Features.Get<OutputCacheProfile>();
    }



    /// <summary>
    /// Enabled output caching of the response.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="slidingExpiration">The amount of seconds to cache the output for.</param>
    /// <param name="varyByHeaders">Comma separated list of HTTP headers to vary the caching by.</param>
    /// <param name="varyByParam">Comma separated list of query string parameter names to vary the caching by.</param>
    /// <param name="useAbsoluteExpiration">Use absolute expiration instead of the default sliding expiration.</param>
    /// <param name="fileDependencies">Globbing patterns</param>
    public static void EnableOutputCaching(this HttpContext context, TimeSpan slidingExpiration, string varyByHeaders = null, string varyByParam = null, bool useAbsoluteExpiration = false, params string[] fileDependencies)
    {
      OutputCacheProfile feature = context.Features.Get<OutputCacheProfile>();

      if (feature == null)
      {
        feature = new OutputCacheProfile();
        context.Features.Set(feature);
      }

      feature.Duration = slidingExpiration.TotalSeconds;
      feature.FileDependencies = fileDependencies;
      feature.VaryByHeader = varyByHeaders;
      feature.VaryByParam = varyByParam;
      feature.UseAbsoluteExpiration = useAbsoluteExpiration;
    }

    internal static bool IsOutputCachingEnabled(this HttpContext context, out OutputCacheProfile profile)
    {
      profile = context.Features.Get<OutputCacheProfile>();

      return profile != null && profile.Duration > 0;
    }
  }
}
