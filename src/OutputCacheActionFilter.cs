using DonutOutputCachingCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Microsoft.AspNetCore.Mvc
{
  /// <summary>
  /// Enables server-side output caching.
  /// </summary>
  public class DonutOutputCacheAttribute : ActionFilterAttribute
  {

    private readonly string[] _fileDependencies;

    /// <summary>
    /// Enables server-side output caching.
    /// </summary>
    /// <param name="fileDependencies">Globbing patterns relative to the content root (not the wwwroot).</param>
    public DonutOutputCacheAttribute(params string[] fileDependencies)
    {
      _fileDependencies = fileDependencies;

      if (fileDependencies.Length == 0)
      {
        _fileDependencies = new[] { "**/*.cs", "**/*.cshtml" };
      }
    }

    /// <summary>
    /// The name of the profile.
    /// </summary>
    public string Profile { get; set; }

    /// <summary>
    /// The duration in seconds of how long to cache the response.
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// Comma separated list of HTTP headers to vary the caching by.
    /// </summary>
    public string VaryByHeader { get; set; }

    /// <summary>
    /// Comma separated list of query string parameters to vary the caching by.
    /// </summary>
    public string VaryByParam { get; set; }

    /// <summary>
    /// Use absolute expiration instead of the default sliding expiration.
    /// By default to true
    /// </summary>
    public bool UseAbsoluteExpiration { get; set; } = true;

    /// <summary>
    /// Return the cache handler value
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private OutputCacheHandler GetCacheHandler(HttpContext context)
    {
      if (_cacheHandler == null)
        _cacheHandler = (OutputCacheHandler)context.RequestServices.GetService(typeof(OutputCacheHandler));

      return _cacheHandler;
    }

    /// <summary>
    /// Current cache entry
    /// </summary>
    private OutputCacheResponseEntry _entry { get; set; }


    /// <summary>
    /// Cache handler
    /// </summary>
    private OutputCacheHandler _cacheHandler { get; set; }


    /// <summary>
    /// Executing the filter 
    /// This will return the cached result if it's in cache. 
    /// I moved the cache handfling because viewcomponent requires a ControllerContext for a proper execution with RouteData... 
    /// Unfortunatly, you're unable to cache the ActionContext from a middleware ...
    /// </summary>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
      var cacheHandler = GetCacheHandler(context.HttpContext);


      // Set the current profile
      cacheHandler.GetOrSetProfile(context, Profile, new OutputCacheProfile()
      {
        Duration = Duration,
        VaryByHeader = VaryByHeader,
        VaryByParam = VaryByParam,
        FileDependencies = _fileDependencies,
        UseAbsoluteExpiration = UseAbsoluteExpiration
      });

      var cache = (IOutputCachingService)context.HttpContext.RequestServices.GetService(typeof(IOutputCachingService));

      OutputCacheResponseEntry entry;

      if (cache.TryGetValue(context.HttpContext.Request.Host + context.HttpContext.Request.Path, out entry) && entry.IsCached(context.HttpContext, out OutputCacheResponse response))
      {
        context.Result = cacheHandler.Get(context, response).Result;
      }
      else
        base.OnActionExecuting(context);

    }


    
    /// <summary>
    /// Executing the filter
    /// </summary>
    public override void OnActionExecuted(ActionExecutedContext context)
    {
      var cacheHandler = GetCacheHandler(context.HttpContext);

      // anyway remove the donutoutput cache tags in the response
      cacheHandler.RemoveDonutOutputCacheTags(context.HttpContext);

      base.OnActionExecuted(context);
    }
  }
}
