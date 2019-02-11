using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DonutOutputCachingCore
{
  internal class DistributedOutputCachingService : IOutputCachingService
  {
    private IDistributedCache _cache;

    public DistributedOutputCachingService(IDistributedCache cache)
    {
      _cache = cache;
    }

    public bool TryGetValue(string route, out OutputCacheResponseEntry value)
    {
      string cleanRoute = NormalizeRoute(route);

      var bytes = _cache.Get(cleanRoute);

      if (bytes != null && bytes.Length  > 0) { 

        value = ByteArrayToObject(bytes) as OutputCacheResponseEntry;
        return true;
      }

      value = null;
      return false;
    }



    public void Set(string route, OutputCacheResponseEntry entry, HttpContext context)
    {
      if (!context.IsOutputCachingEnabled(out OutputCacheProfile profile))
        return;

      var env = (IHostingEnvironment)context.RequestServices.GetService(typeof(IHostingEnvironment));

      DistributedCacheEntryOptions options = GetDistributedCacheOption(profile, env);

      if (entry != null) {  
        string cleanRoute = NormalizeRoute(route);
         _cache.Set(cleanRoute, ObjectToByteArray(entry), options);
      }
    }


    private DistributedCacheEntryOptions GetDistributedCacheOption(OutputCacheProfile profile, IHostingEnvironment env)
    {
      var options = new DistributedCacheEntryOptions();
      if (profile.UseAbsoluteExpiration)
      {
        options.SetAbsoluteExpiration(TimeSpan.FromSeconds(profile.Duration));
      }
      else
      {
        options.SetSlidingExpiration(TimeSpan.FromSeconds(profile.Duration));
      }

      return options;
    }

    public void Remove(string route)
    {
      string cleanRoute = NormalizeRoute(route);
      _cache.Remove(cleanRoute);
    }


    public void Clear()
    {
      throw new NotImplementedException();
    }

    private static string NormalizeRoute(string route)
    {
      return "/" + route.Trim().Trim('/');
    }


    // Convert an object to a byte array
    private byte[] ObjectToByteArray(Object obj)
    {
      if (obj == null)
        return null;

      BinaryFormatter bf = new BinaryFormatter();
      MemoryStream ms = new MemoryStream();
      bf.Serialize(ms, obj);

      return ms.ToArray();
    }

    // Convert a byte array to an Object
    private Object ByteArrayToObject(byte[] arrBytes)
    {
      MemoryStream memStream = new MemoryStream();
      BinaryFormatter binForm = new BinaryFormatter();
      memStream.Write(arrBytes, 0, arrBytes.Length);
      memStream.Seek(0, SeekOrigin.Begin);
      Object obj = (Object)binForm.Deserialize(memStream);

      return obj;
    }
  }
}