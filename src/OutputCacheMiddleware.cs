using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.IO;
using System.Threading.Tasks;

namespace DonutOutputCachingCore
{
  internal class DonutOutputCacheMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly IOutputCachingService _cache;
    private readonly OutputCacheOptions _options;
    private readonly DonutRenderingService _donutCacheHandler;

    public DonutOutputCacheMiddleware(RequestDelegate next, IOutputCachingService cache, OutputCacheOptions options, DonutRenderingService donutCacheHandler)
    {
      _next = next;
      _cache = cache;
      _donutCacheHandler = donutCacheHandler;
      _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      if (!_options.DoesRequestQualify(context))
      {
        await _next(context);
      }
      else
      {
        await ServeFromMvcAndCacheAsync(context, null);
      }
    }

    private async Task ServeFromMvcAndCacheAsync(HttpContext context, OutputCacheResponseEntry entry)
    {


      // If cache not set before

      HttpResponse response = context.Response;
      Stream originalStream = response.Body;

      using (var ms = new MemoryStream())
      {
        response.Body = ms;
        await _next(context);
        byte[] bytes = ms.ToArray();

        if (!context.Response.Headers.ContainsKey(HeaderNames.ETag) && context.IsOutputCachingEnabled(out OutputCacheProfile profile) && _options.DoesResponseQualify(context))
        {
          context.AddEtagToResponse(bytes);
          AddResponseToCache(context, entry, bytes);
        }

        if (ms.Length > 0)
        {
          var responseWithoutDonutTags = await _donutCacheHandler.RemoveDonutHtmlTags(bytes);
          response.Headers.ContentLength = responseWithoutDonutTags.Length;
          using (var tempStream = new MemoryStream(responseWithoutDonutTags))
          {
            tempStream.Seek(0, SeekOrigin.Begin);
            tempStream.CopyTo(originalStream);
          }
        }
        response.Body = originalStream;

      }
    }

    private async Task ServeFromCacheAsync(HttpContext context, OutputCacheResponse value)
    {

      // Copy over the HTTP headers
      foreach (string name in value.Headers.Keys)
      {
        if (!context.Response.Headers.ContainsKey(name))
        {
          context.Response.Headers[name] = value.Headers[name];
        }
      }

      var body = await _donutCacheHandler.RemoveDonutHtmlTags(value.Body);
      context.Response.ContentLength = body.Length;
      await context.Response.Body.WriteAsync(body, 0, body.Length);
    }

    private void AddResponseToCache(HttpContext context, OutputCacheResponseEntry entry, byte[] bytes)
    {
      if (!context.IsOutputCachingEnabled(out OutputCacheProfile profile))
      {
        return;
      }

      if (entry == null)
      {
        entry = new OutputCacheResponseEntry(context, bytes, profile);
        _cache.Set(context.Request.Host + context.Request.Path, entry, context);
      }
      else
      {
        entry.AddResponse(context, new OutputCacheResponse(bytes, context.Response.Headers));
      }
    }

  }
}