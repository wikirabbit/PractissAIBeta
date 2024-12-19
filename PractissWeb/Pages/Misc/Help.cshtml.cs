using ApiIntegrations.Misc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Misc
{
    public class HelpTourModel : PageModel
    {
        public void OnGet()
        {
        }

		public async Task<IActionResult> OnPost(string message)
		{
			var userId = HttpContext.Session.GetString("UserId");
			var user = await PractissApiClientLibrary.GetUserAsync(userId);

			// Process the message here (e.g., save to database, send an email)
			SendgridClientLibrary.SendEmailToPractissTeam(user, "ContactUs", message);

			TempData["Message"] = "Thank you for your message. We'll get back to you soon.";
			return RedirectToPage();
		}
	}
}
