using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;

namespace DonutOutputCachingCore.CacheHoleAttribute
{
    public class DonutHoleComponent
    {
        public string Name { get; set; }
        public object Arguments { get; set; }
        public IHtmlContent Result { get; set; }
    }
}
