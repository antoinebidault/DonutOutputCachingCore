using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;

namespace DonutOutputCachingCore
{
  /// <summary>
  /// A cache profile.
  /// </summary>
  [Serializable]
  public class OutputCacheProfile
  {
    /// <summary>
    /// The duration in seconds of how long to cache the response.
    /// </summary>
    public double Duration { get; set; }

    /// <summary>
    /// Comma separated list of HTTP headers to vary the caching by.
    /// </summary>
    public string VaryByHeader { get; set; } = HeaderNames.AcceptEncoding;

    /// <summary>
    /// Comma separated list of query string parameters to vary the caching by.
    /// </summary>
    public string VaryByParam { get; set; }

    /// <summary>
    /// Globbing patterns relative to the content root (not the wwwroot).
    /// </summary>
    public IEnumerable<string> FileDependencies { get; set; } = new[] { "**/*.*" };

    /// <summary>
    /// Use absolute expiration instead of the default sliding expiration.
    /// </summary>
    public bool UseAbsoluteExpiration { get; set; }

  }
}
