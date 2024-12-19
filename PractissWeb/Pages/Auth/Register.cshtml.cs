using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Auth
{
	public class RegisterModel : PageModel
    {
        [BindProperty]
        public string FirstName { get; set; }

        [BindProperty]
        public string LastName { get; set; }

        [BindProperty]
        public string Email { get; set; }


        [BindProperty]
        public bool TermsAccepted { get; set; }

        public void OnGet()
        {
            Email = HttpContext.Session.GetString("NewUserEmail");
        }

        public async Task<IActionResult> OnPost()
        {
            ModelState.Remove("Email");
            Email = HttpContext.Session.GetString("NewUserEmail");

			if (!ModelState.IsValid || !TermsAccepted)
            {
                // Handle the case where the model is not valid or terms are not accepted
                return Page();
            }

            User user = await PractissApiClientLibrary.GetUserByEmailAsync(Email);

            if (user == null)
            {
                user = new User
				{
					Id = Guid.NewGuid().ToString(),
					FirstName = FirstName,
					LastName = LastName,
					Email = Email,
					Roles = "learner"
				};

				await PractissApiClientLibrary.CreateUserAsync(user);
			}

            return RedirectToPage("/Auth/Login");
        }
    }
}
