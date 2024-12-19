using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PractissWeb.Pages
{
    public class BasePageModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BasePageModel(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            base.OnPageHandlerExecuting(context);

            // Check if the session contains 'UserId'
            if (context.HttpContext.Session.GetString("UserId") == null)
            {
                // Store the incoming URL in the session under 'OriginalUrl'
                var originalUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.HttpContext.Session.SetString("OriginalUrl", originalUrl);

                // If 'UserId' not present, redirect to login page
                context.Result = new RedirectToPageResult("/Auth/Login");
            }
        }

        public string GetVersionedUrl(string filePath)
        {
            // Ensure the filePath does not start with '~/' since WebRootPath already points to wwwroot
            // Also ensure there's a leading slash
            if (filePath.StartsWith("~/"))
            {
                filePath = filePath.Substring(2); // Remove the '~/'
            }
            if (!filePath.StartsWith("/"))
            {
                filePath = "/" + filePath; // Ensure there's a leading slash
            }

            var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('/'));
            var version = System.IO.File.GetLastWriteTime(physicalPath).ToString("yyyyMMddHHmmss");
            return filePath + "?v=" + version;
        }

    }
}
