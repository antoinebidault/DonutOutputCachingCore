using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DonutOutputCachingCore
{
  internal static class HttpContextExtensions
  {

    internal static void AddEtagToResponse(this HttpContext context, byte[] bytes)
    {
      if (context.Response.StatusCode != StatusCodes.Status200OK)
        return;

      if (!context.IsOutputCachingEnabled(out OutputCacheProfile profile))
      {
        return;
      }

      if (context.Response.Headers.ContainsKey(HeaderNames.ETag))
        return;


      context.Response.Headers[HeaderNames.CacheControl] = "public, max-age=" + profile.Duration;
      context.Response.Headers[HeaderNames.ETag] = context.Request.CalculateChecksum(bytes);
    }


    internal static string CalculateChecksum(this HttpRequest request, byte[] bytes)
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
