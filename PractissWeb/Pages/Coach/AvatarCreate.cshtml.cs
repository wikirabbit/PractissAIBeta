using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Coach
{
    public class AvatarCreateModel : BasePageModel
    {
        [BindProperty]
        public string AvatarName { get; set; }
        [BindProperty]
        public string AvatarImageCode { get; set; }
        [BindProperty]
        public string AvatarVoice { get; set; }
        public bool ShowKidsAvatars { get; set; }
		public bool ShowIntenseAvatars { get; set; }

		public AvatarCreateModel(IWebHostEnvironment webHostEnvironment)
            : base(webHostEnvironment)
        {
        }

        public async Task OnGet()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var user = await PractissApiClientLibrary.GetUserAsync(userId);
            if (user.AdminPrompts.CustomField != null && (user.AdminPrompts.CustomField.ToLower().Contains("kids")))
            {
                ShowKidsAvatars = true;
            }
            if (user.ShowIntenseAvatars)
            {
                ShowIntenseAvatars = true;
            }


            // Set default values here. Replace these with actual values as needed
            AvatarName = "Name";
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = HttpContext.Session.GetString("UserId");

            var avatar = new Avatar
            {
                // Set properties from the form data
                Name = AvatarName,
                Image = Helpers.GetBase64StringForImage("/img/avatars/reference-" + AvatarImageCode + ".png"),
                ImageCode = AvatarImageCode,
                VoiceName = AvatarVoice.Split('-')[0],
                AuthorId = userId,
                Personality = AvatarVoice.Split('-').Length > 1 ? AvatarVoice.Split('-')[1] : null,
			    Id = Guid.NewGuid().ToString()
            };

            //new AvatarService().CreateAvatarAsync(avatar);
            await PractissApiClientLibrary.CreateAvatarAsync(avatar);

            // After handling the form, you can redirect the user to another page or reload the current page
            return RedirectToPage("/Coach/AvatarLibrary"); // Redirect to a success page or any other page
        }
    }
}
