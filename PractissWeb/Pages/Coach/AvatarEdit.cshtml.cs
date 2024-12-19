using Microsoft.AspNetCore.Mvc;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Coach
{
    public class AvatarEditModel : BasePageModel
    {
        [BindProperty]
        public string AvatarName { get; set; }
        public string AvatarImage { get; set; }
        [BindProperty]
        public string AvatarImageCode { get; set; }
        [BindProperty]
        public string AvatarVoice { get; set; }
		public bool ShowKidsAvatars { get; set; }
		public bool ShowIntenseAvatars { get; set; }

		public AvatarEditModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task OnGet(string avatarId)
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


			// Fetch the avatar details using avatarId
			var avatar = await PractissApiClientLibrary.GetAvatarAsync(avatarId);

            if (avatar != null)
            {
                AvatarName = avatar.Name;
                AvatarImage = avatar.Image;
                AvatarImageCode = avatar.ImageCode;
                AvatarVoice = avatar.VoiceName.ToLowerInvariant();
                if(!String.IsNullOrEmpty(avatar.Personality))
                {
                    AvatarVoice += "-" + avatar.Personality;
                }
            }
        }

        public async Task<IActionResult> OnPost(string avatarId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var avatar = await PractissApiClientLibrary.GetAvatarAsync(avatarId);

            avatar.Name = AvatarName;
            avatar.Image = Helpers.GetBase64StringForImage("/img/avatars/reference-" + AvatarImageCode + ".png");
            avatar.ImageCode = AvatarImageCode;
            avatar.VoiceName = AvatarVoice.Split('-')[0];
            avatar.Personality = AvatarVoice.Split('-').Length > 1 ? AvatarVoice.Split('-')[1] : null;

            await PractissApiClientLibrary.UpdateAvatarAsync(avatarId, avatar);

            // After handling the form, you can redirect the user to another page or reload the current page
            return RedirectToPage("/Coach/AvatarLibrary"); // Redirect to a success page or any other page
        }
    }
}
