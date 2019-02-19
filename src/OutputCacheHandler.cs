using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DonutOutputCachingCore
{
  internal class OutputCacheHandler
  {
    private readonly IOutputCachingService _cache;
    private readonly OutputCacheOptions _options;
    private readonly DonutRenderingService _donutCacheHandler;

    public OutputCacheHandler(IOutputCachingService cache, OutputCacheOptions options, DonutRenderingService donutCacheHandler)
    {
      _cache = cache;
      _donutCacheHandler = donutCacheHandler;
      _options = options;
    }


    /// <summary>
    /// Serve the response from cache
    /// </summary>
    /// <param name="context"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<ContentResult> Get(ActionExecutingContext context, OutputCacheResponse value)
    {

      // Copy over the HTTP headers
      foreach (string name in value.Headers.Keys)
      {
        if (!context.HttpContext.Response.Headers.ContainsKey(name))
        {
          context.HttpContext.Response.Headers[name] = value.Headers[name];
        }
      }

      // Execution of the child's viewcomponents
      var body = await _donutCacheHandler.ParseAndExecuteViewComponentsAsync(context, value.Body);
      body = await _donutCacheHandler.RemoveDonutHtmlTags(body);

      return new ContentResult
      {
        Content = System.Text.Encoding.UTF8.GetString(body)
      };
    }

  
    internal void Set(HttpContext context, OutputCacheResponseEntry entry) 
    {
      if (_options.DoesResponseQualify(context))
      {
        byte[] bytes = ReadFully(context.Response.Body);

        context.AddEtagToResponse(bytes);
        AddResponseToCache(context, entry, bytes);
      }
    }


    public void GetOrSetProfile(ActionExecutingContext context, string profileKey, OutputCacheProfile newProfile)
    {
      OutputCacheProfile profile = null;
      if (!string.IsNullOrEmpty(profileKey))
      {
        if (_options == null || !_options.Profiles.ContainsKey(profileKey))
        {
          throw new ArgumentException($"The Profile '{profileKey}' hasn't been created.");
        }

        profile = _options.Profiles[profileKey];

      }
      else
      {
        profile = newProfile;
      }

      context.HttpContext.Features.Set<OutputCacheProfile>(profile);
    }

    private byte[] ReadFully(Stream input)
    {
      input.Seek(0, SeekOrigin.Begin);
      using (MemoryStream ms = new MemoryStream())
      {
        input.CopyTo(ms);
        return ms.ToArray();
      }
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

    /// <summary>
    /// Remove the <donutoutputcaching></donutoutputcaching> tags
    /// </summary>
    /// <param name="context"></param>
    internal async void RemoveDonutOutputCacheTags(HttpContext context)
    {
      byte[] bytes = ReadFully(context.Response.Body);
      if (bytes.Length > 0)
      {
        var responseWithoutDonutTags = await _donutCacheHandler.RemoveDonutHtmlTags(bytes);
        context.Response.Headers.ContentLength = responseWithoutDonutTags.Length;
        using (var tempStream = new MemoryStream(responseWithoutDonutTags))
        {
          tempStream.Seek(0, SeekOrigin.Begin);
          tempStream.CopyTo(context.Response.Body);
        }
      }
    }

  }
}