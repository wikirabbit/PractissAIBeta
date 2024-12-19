using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Coach
{
    public class ModuleEditModel : BasePageModel
    {
        [BindProperty]
        public string ModuleName { get; set; }

        [BindProperty]
        public string ModuleDescription { get; set; }

        [BindProperty]
        public string SelectedAvatar { get; set; }

        [BindProperty]
        public string Situation { get; set; }

        [BindProperty]
        public string Evaluation { get; set; }


        [BindProperty]
        public bool IsPublic { get; set; }

        // Property to hold avatar options
        public List<SelectListItem> AvatarOptions { get; set; }

        public ModuleEditModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task OnGet(string moduleId)
        {
            var module = await PractissApiClientLibrary.GetModuleAsync(moduleId);
            if (module != null)
            {
                ModuleName = module.Title;
                ModuleDescription = module.Description;
                Situation = module.Situation;
                Evaluation = module.Evaluation;
                IsPublic = module.Visibility == "Public";
                SelectedAvatar = module.Avatar.Name;

                var userId = HttpContext.Session.GetString("UserId");
                var avatars = await PractissApiClientLibrary.GetAvatarsByAuthorIdAsync(userId);

                AvatarOptions = new List<SelectListItem> { };
                foreach (var avatar in avatars)
                {
                    AvatarOptions.Add(new SelectListItem { Value = avatar.Name, Text = avatar.Name });
                }
            }
        }

        public async Task<IActionResult> OnPost(string moduleId)
        {
            ModelState.Remove("Evaluation");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var module = await PractissApiClientLibrary.GetModuleAsync(moduleId);

            module.Title = ModuleName;
            module.Description = ModuleDescription;
            module.Situation = Situation;
            module.Evaluation = Evaluation;
            module.Visibility = (IsPublic ? ModuleVisibility.Public : ModuleVisibility.Private).ToString();

            var authorId = HttpContext.Session.GetString("UserId");
            var avatars = await PractissApiClientLibrary.GetAvatarsByAuthorIdAsync(authorId);
            foreach (var avatar in avatars)
            {
                if (avatar.Name == SelectedAvatar)
                    module.Avatar = avatar;
            }

            PractissApiClientLibrary.UpdateModuleAsync(module).Wait();

            return RedirectToPage("/Coach/ModuleLibrary"); // Redirect to a success page
        }
    }
}
