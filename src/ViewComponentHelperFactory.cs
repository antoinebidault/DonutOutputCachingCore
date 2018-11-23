using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

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
        public IViewComponentHelper Create(HttpContext context)
        {
            var modelMetadataProvider = context.RequestServices.GetRequiredService<IModelMetadataProvider>();
            var tempDataProvider = context.RequestServices.GetRequiredService<ITempDataProvider>();
            var htmlHelper = context.RequestServices.GetRequiredService<IViewComponentHelper>();
            var viewContext = new ViewContext(
                new ActionContext(context,new RouteData(), new ControllerActionDescriptor()),
                new FakeView(),
                new ViewDataDictionary(modelMetadataProvider, new ModelStateDictionary()),
                new TempDataDictionary(context, tempDataProvider),
                TextWriter.Null,
                new HtmlHelperOptions()
            );

            ((IViewContextAware)htmlHelper).Contextualize(viewContext);
            return htmlHelper;
        }
    }
}
