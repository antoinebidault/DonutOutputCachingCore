using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;

namespace DonutOutputCachingCore.CacheHoleAttribute
{
  /// <summary>
  /// This factory will instantiate a IViewComponentHelper based on httpContext with a fake ViewContext
  /// </summary>
  public class ViewComponentHelperFactory
  {
    public ViewComponentHelperFactory()
    {
    }

    public class FakeView : IView
    {
      /// <inheritdoc />
      public Task RenderAsync(ViewContext context)
      {
        return Task.CompletedTask;
      }

      /// <inheritdoc />
      public string Path { get; } = "View";
    }


    /// <inheritdoc />
    public IViewComponentHelper Create(ActionExecutingContext context)
    {
      var http = context.HttpContext;
      var modelMetadataProvider = http.RequestServices.GetRequiredService<IModelMetadataProvider>();
      var tempDataProvider = http.RequestServices.GetRequiredService<ITempDataProvider>();
      var htmlHelper = http.RequestServices.GetRequiredService<IViewComponentHelper>();
      var viewContext = new ViewContext(
          new ActionContext(http, context.RouteData, context.ActionDescriptor),
          new FakeView(),
          new ViewDataDictionary(modelMetadataProvider, new ModelStateDictionary()),
          new TempDataDictionary(http, tempDataProvider),
          TextWriter.Null,
          new HtmlHelperOptions()
      );

      ((IViewContextAware)htmlHelper).Contextualize(viewContext);
      return htmlHelper;
    }
  }
}
