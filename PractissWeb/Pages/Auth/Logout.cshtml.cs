using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PractissWeb.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            HttpContext.Session.Remove("UserId");

            if (HttpContext.Request.Cookies["UserId"] != null)
            {
                // To delete the cookie, set its expiration to a past date
                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddDays(-1); // Set to yesterday
                HttpContext.Response.Cookies.Append("UserId", "", options);
            }

            return RedirectToPage("/Auth/Login");
        }
    }
}
