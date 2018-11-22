using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace Sample.ViewComponents
{
    public class TestViewComponent : ViewComponent
    {
        public TestViewComponent()
        {
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return await Task.FromResult(View());
        }
    }
}
