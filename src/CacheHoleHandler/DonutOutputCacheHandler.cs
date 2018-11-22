using DonutOutputCachingCore.CacheHoleAttribute;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc
{
  internal class DonutOutputCacheHandler
  {
    private ViewComponentHelperFactory _factory;
    public DonutOutputCacheHandler(ViewComponentHelperFactory factory)
    {
      _factory = factory;
    }

    internal IViewComponentHelper GetViewComponentHelper(HttpContext context)
    {
      return _factory.Create(context);
    }

    internal async Task<byte[]> ParseAndRemoveTheOutputCacheTags( byte[] htmlBytes)
    {
      string htmlString = System.Text.Encoding.UTF8.GetString(htmlBytes);
      HtmlDocument htmlDoc = LoadDocument(htmlString);
      var nodes = htmlDoc.DocumentNode.SelectNodes("descendant::donutoutputcache");
      if (nodes == null || nodes.Count == 0)
        return htmlBytes;

      foreach (var node in nodes)
      {
        node.ParentNode.RemoveChild(node, true);
      }

      return await Task.FromResult(StringToByteArray(htmlDoc.DocumentNode.OuterHtml));
    }



    internal async Task<byte[]> ParseAndExecuteChildRequestAsync(HttpContext context, byte[] htmlBytes)
    {
      object argsObject;
      string htmlString = System.Text.Encoding.UTF8.GetString(htmlBytes);
      HtmlDocument htmlDoc = LoadDocument(htmlString);
      var nodes = htmlDoc.DocumentNode.SelectNodes("descendant::donutoutputcache");

      if (nodes == null || nodes.Count == 0)
        return htmlBytes;

      var donutHoles = new List<DonutHoleComponent>();
      var helper = GetViewComponentHelper(context);

      foreach (HtmlNode node in nodes)
      {
        var name = node.Attributes["data-name"].Value;
        var args = node.Attributes["data-args"]?.Value;
        argsObject = args != null ? JsonConvert.DeserializeObject(args) : null;

        var result = await helper.InvokeAsync(name, arguments: argsObject);

        htmlString = htmlString.Replace(node.InnerHtml, result?.GetString());
      }


      return await Task.FromResult(StringToByteArray(htmlString));

    }

    private static HtmlDocument LoadDocument(string htmlString)
    {
      HtmlDocument htmlDoc;
      htmlDoc = new HtmlDocument();
      htmlDoc.LoadHtml(htmlString);
      return htmlDoc;
    }

    private byte[] StringToByteArray(string input)
    {
      return Encoding.ASCII.GetBytes(input);
    }

  }
}
