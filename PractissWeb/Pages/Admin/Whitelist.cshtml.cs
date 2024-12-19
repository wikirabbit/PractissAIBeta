using Microsoft.AspNetCore.Mvc;
using PractissWeb.Utilities;
using System.ComponentModel.DataAnnotations;

namespace PractissWeb.Pages.Admin
{
	public class WhitelistModel : BasePageModel
    {

        [BindProperty]
        [Required(ErrorMessage = "First name is required.")]
        public string FirstNameToAdd { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Last name is required.")]
        public string LastNameToAdd { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Email is required.")]
        public string EmailToAdd { get; set; }

        public List<CommonTypes.User> Users { get; set; }

        public WhitelistModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task OnGet()
        {
            Users = await PractissApiClientLibrary.GetAllUsersAsync();
            Users = Users.Where(u => u.UserStatus.IsInactive == false).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> OnGetNameFromEmail([FromQuery] string email)
        {
            var user = await PractissApiClientLibrary.GetUserByEmailAsync(email);
            if (user != null)
            {
                return new JsonResult(new { isUserFound = true, firstName = user.FirstName, lastName = user.LastName });
            }
            else
            {
                return new JsonResult(new { isUserFound = false, firstName = string.Empty, lastName = string.Empty });
            }
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!string.IsNullOrWhiteSpace(EmailToAdd))
            {
                // Create a learner if they don't exist in the system
                var learner = await PractissApiClientLibrary.GetUserByEmailAsync(EmailToAdd);
                if (learner == null)
                {
                    learner = new CommonTypes.User()
                    {
                        Id = Guid.NewGuid().ToString(),
                        FirstName = FirstNameToAdd,
                        LastName = LastNameToAdd,
                        Email = EmailToAdd,
                        Roles = "learner designer"
                    };

                    await PractissApiClientLibrary.CreateUserAsync(learner);
                }

                Users = await PractissApiClientLibrary.GetAllUsersAsync();
                Users = Users.Where(u => u.UserStatus.IsInactive == false).ToList();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
        {
            var users = await PractissWeb.Utilities.PractissApiClientLibrary.GetAllUsersAsync();
            var user = users.Where(x => x.Id == userId).First();
            user.UserStatus.IsInactive = true;
            await PractissApiClientLibrary.UpdateUserAsync(user);
            return new JsonResult(new { success = true });
        }
    }
}
