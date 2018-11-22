// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    /// <summary>
    /// Extension methods for <see cref="IViewComponentHelper"/>.
    /// </summary>
    public static class ViewComponentHelperExtensions
    {

        public static Task<IHtmlContent> InvokeAsync(this IViewComponentHelper helper, string name, bool excludeFromCache = false)
        {
            return helper.InvokeAsync(name, null, excludeFromCache);
        }

        /// <summary>
        /// Invokes a view component with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="helper">The <see cref="IViewComponentHelper"/>.</param>
        /// <param name="name">The name of the view component.</param>
        /// <returns>A <see cref="Task"/> that on completion returns the rendered <see cref="IHtmlContent" />.
        /// </returns>
        public static Task<IHtmlContent> InvokeAsync(this IViewComponentHelper helper, string name, object arguments = null, bool excludeFromCache = false)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }


            if (excludeFromCache)
            {
                var outputCache = helper.InvokeAsync(name, arguments: arguments, excludeFromCache: false).Result.GetString();
                var jsonStrAttribute = arguments != null ? JsonConvert.SerializeObject(arguments) : string.Empty;
                var result = $"<donutoutputcache class=\"StartDonutOutputCaching\" data-name=\"{name}\" data-args=\"{jsonStrAttribute}\">{outputCache}</donutoutputcache>";
                return Task.FromResult(new HtmlString(result) as IHtmlContent);
            }
            else
            {
                return helper.InvokeAsync(name, arguments: arguments);
            }
        }

        /// <summary>
        /// Transform content to html
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string GetString(this IHtmlContent content)
        {
            var writer = new System.IO.StringWriter();
            content.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }
    }
}