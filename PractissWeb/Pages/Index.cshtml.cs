using Microsoft.AspNetCore.Mvc;
using PractissWeb.Utilities;

namespace PractissWeb.Pages
{
    public class IndexModel : BasePageModel
    {
        public IndexModel(IWebHostEnvironment webHostEnvironment)
    : base(webHostEnvironment)
        {
        }

        public async Task<IActionResult> OnGet()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
                return RedirectToPage("/Auth/Login");

            var user = await PractissApiClientLibrary.GetUserAsync(userId);

            // Check if 'OriginalUrl' is stored in session
            var originalUrl = HttpContext.Session.GetString("OriginalUrl");
            if (!string.IsNullOrEmpty(originalUrl))
            {
                // Clear the 'OriginalUrl' session after retrieving it
                HttpContext.Session.Remove("OriginalUrl");

                // Redirect to the original URL
                return Redirect(originalUrl);
            }
            else
            {
                user.LastLoginTime = DateTime.UtcNow.ToString("o");
                PractissApiClientLibrary.UpdateUserAsync(user);

                if (user.Roles != null && user.Roles.Contains("designer"))
                    return RedirectToPage("/Coach/ModuleLibrary");
                else
                    return RedirectToPage("/Learner/Modules");
            }
        }
    }
}