using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DonutOutputCachingCore
{
  internal class DonutOutputCacheMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly IOutputCachingService _cache;
    private readonly OutputCacheOptions _options;
    private readonly DonutOutputCacheHandler _donutCacheHandler;

    public DonutOutputCacheMiddleware(RequestDelegate next, IOutputCachingService cache, OutputCacheOptions options, DonutOutputCacheHandler donutCacheHandler)
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
      else if (_cache.TryGetValue(context.Request.Host + context.Request.Path, out OutputCacheResponseEntry entry) && entry.IsCached(context, out OutputCacheResponse item))
      {
        // Execution of the childs ViewComponents
        item.Body = await _donutCacheHandler.ParseAndExecuteViewComponentsAsync(context, item.Body);
        await ServeFromCacheAsync(context, item);
      }
      else
      {
        await ServeFromMvcAndCacheAsync(context, entry);
      }
    }

    private async Task ServeFromMvcAndCacheAsync(HttpContext context, OutputCacheResponseEntry entry)
    {
      HttpResponse response = context.Response;
      Stream originalStream = response.Body;

      try
      {
        using (var ms = new MemoryStream())
        {
          response.Body = ms;

          await _next(context);

          if (_options.DoesResponseQualify(context))
          {
            byte[] bytes = ms.ToArray();

            AddEtagToResponse(context, bytes);
            AddResponseToCache(context, entry, bytes);
          }

          // H
          if (ms.Length > 0)
          {
            var responseWithoutDonutTags = await _donutCacheHandler.RemoveDonutHtmlTags(ms.ToArray());
            response.Headers.ContentLength = responseWithoutDonutTags.Length;
            using (var tempStream = new MemoryStream(responseWithoutDonutTags))
            {
              tempStream.Seek(0, SeekOrigin.Begin);
              tempStream.CopyTo(originalStream);
            }
          }
        }
      }
      finally
      {
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

    private static void AddEtagToResponse(HttpContext context, byte[] bytes)
    {
      if (context.Response.StatusCode != StatusCodes.Status200OK)
        return;

      if (!context.IsOutputCachingEnabled(out OutputCacheProfile profile))
      {
        return;
      }

      if (context.Response.Headers.ContainsKey(HeaderNames.ETag))
        return;

      context.Response.Headers[HeaderNames.ETag] = CalculateChecksum(bytes, context.Request);
    }

    private static string CalculateChecksum(byte[] bytes, HttpRequest request)
    {
      byte[] encoding = Encoding.UTF8.GetBytes(request.Headers[HeaderNames.AcceptEncoding].ToString());

      using (var algo = SHA1.Create())
      {
        byte[] buffer = algo.ComputeHash(bytes.Concat(encoding).ToArray());
        return $"\"{WebEncoders.Base64UrlEncode(buffer)}\"";
      }
    }
  }
}