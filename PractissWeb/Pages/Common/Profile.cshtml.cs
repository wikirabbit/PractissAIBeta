using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Common
{
    public class ProfileModel : BasePageModel
    {
        [BindProperty]
        public string FirstName { get; set; }

        [BindProperty]
        public string LastName { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Bio { get; set; }

        public string ProfileImage { get; set; }

        [BindProperty]
        public IFormFile UploadedImage { get; set; }

        [BindProperty]
        public bool IsInstructionDesigner { get; set; }
        [BindProperty]
        public bool ShowIntenseAvatars { get; set; }
        [BindProperty]
        public bool StreamingMode { get; set; }

        #region Integrations

        [BindProperty]
        public Integrations ApiIntegrations { get; set; }

        #endregion

        [BindProperty]
        public LLMType RoleplayLLM { get; set; }

        [BindProperty]
        public LLMType ReportLLM { get; set; }


        public ProfileModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task OnGet()
        {
            var email = HttpContext.Session.GetString("Email");
            var user = await PractissApiClientLibrary.GetUserByEmailAsync(email);
            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.Email;
            Bio = user.Bio;
            ProfileImage = user.ProfileImage;
            IsInstructionDesigner = user.Roles != null && user.Roles.Contains("designer");
			ShowIntenseAvatars = user.ShowIntenseAvatars;
            StreamingMode = user.StreamingMode;
            RoleplayLLM = user.RoleplayLLM;
            ReportLLM = user.ReportLLM;

            if (string.IsNullOrWhiteSpace(ProfileImage))
                ProfileImage = Helpers.GetBase64StringForImage("/img/avatars/reference-blank.png");

            ApiIntegrations = user.Integrations;
        }

        public async Task<IActionResult> OnPost()
        {
            var email = HttpContext.Session.GetString("Email");
            var user = await PractissApiClientLibrary.GetUserByEmailAsync(email);
            user.FirstName = FirstName;
            user.LastName = LastName;
            user.Email = Email;
            user.Bio = Bio;
            user.StreamingMode = StreamingMode;
            user.ShowIntenseAvatars = ShowIntenseAvatars;

            if (IsInstructionDesigner)
            {
                if (string.IsNullOrEmpty(user.Roles))
                {
                    user.Roles = "designer";
                }
                else if (!user.Roles.Contains("designer"))
                {
                    user.Roles += " designer";
                }
            }
            else
            {
				user.Roles = user.Roles.Replace("designer", "");
			}

            HttpContext.Session.SetString("UserRoles", user.Roles);

            if (UploadedImage != null && UploadedImage.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    UploadedImage.CopyToAsync(ms).Wait();
                    byte[] fileBytes = ms.ToArray();
                    user.ProfileImage = Convert.ToBase64String(fileBytes); // Assuming PNG format
                }
            }

            await PractissApiClientLibrary.UpdateUserAsync(user);

            return RedirectToPage("/Common/Profile"); // Redirect to a success page or any other page
        }

        public async Task<IActionResult> OnPostIntegrations()
        {
            var email = HttpContext.Session.GetString("Email");
            var user = await PractissApiClientLibrary.GetUserByEmailAsync(email);
            user.Integrations = ApiIntegrations;


            await PractissApiClientLibrary.UpdateUserAsync(user);

            return RedirectToPage("/Common/Profile"); // Redirect to a success page or any other page
        }

		public async Task<IActionResult> OnPostLLMPreferences()
		{
			var email = HttpContext.Session.GetString("Email");
			var user = await PractissApiClientLibrary.GetUserByEmailAsync(email);

			user.RoleplayLLM = RoleplayLLM;
			user.ReportLLM = ReportLLM;

			await PractissApiClientLibrary.UpdateUserAsync(user);

			return RedirectToPage("/Common/Profile"); // Redirect to a success page or any other page
		}

	}
}
