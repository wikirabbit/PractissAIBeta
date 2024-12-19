using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PractissWeb.Utilities;
using System.ComponentModel.DataAnnotations;

namespace PractissWeb.Pages.Coach
{
	public class ModuleCreateModel : BasePageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Module title is required.")]
        public string ModuleName { get; set; }


        [BindProperty]
        [Required(ErrorMessage = "Module description is required.")]
        public string ModuleDescription { get; set; }


        [BindProperty]
        [Required(ErrorMessage = "An AI roleplay companion is required.")]
        public string SelectedAvatar { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "A description of the AI companion's role is required.")]
        public string Situation { get; set; }

        [BindProperty]
        public string Evaluation { get; set; }

        [BindProperty]
        public bool IsPublic { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Some initial content is required to for the AI to generate the roleplay.")]
        public string RawContent { get; set; }

        // Property to hold avatar options
        public List<SelectListItem> AvatarOptions { get; set; }

        public ModuleCreateModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task OnGet()
        {
            var userId = HttpContext.Session.GetString("UserId");

            // Populate properties with default values or values from a database
            ModuleName = "Provide the name of the module";
            ModuleDescription = StringResources.ModuleDescription_Default;
            Situation = StringResources.ModuleSituation_Default;
            Evaluation = StringResources.ModuleEvaluation_Default;

            var avatars = await PractissApiClientLibrary.GetAvatarsByAuthorIdAsync(userId);

            AvatarOptions = new List<SelectListItem> { };
            foreach (var avatar in avatars)
            {
                AvatarOptions.Add(new SelectListItem { Value = avatar.Name, Text = avatar.Name });
            }
        }

        public async Task<IActionResult> OnPost()
        {
            ModelState.Remove("RawContent");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var authorId = HttpContext.Session.GetString("UserId");
            var author = await PractissApiClientLibrary.GetUserAsync(authorId);

            var module = new Module()
            {
                Id = Guid.NewGuid().ToString(),
                OriginalUserPrompt = RawContent,
                Description = ModuleDescription,
                Author = author,
                Situation = Situation,
                Evaluation = Evaluation,
                Title = ModuleName,
                Visibility = (IsPublic ? ModuleVisibility.Public : ModuleVisibility.Private).ToString()
            };

            var avatars = await PractissApiClientLibrary.GetAvatarsByAuthorIdAsync(authorId);
            foreach (var avatar in avatars)
            {
                if (avatar.Name == SelectedAvatar)
                    module.Avatar = avatar;
            }

            PractissApiClientLibrary.CreateModuleAsync(module).Wait();

            author.UserStats.ModulesCreated++;
            await PractissApiClientLibrary.UpdateUserAsync(author);

            return RedirectToPage("/Coach/ModuleLibrary");
        }

        [HttpGet]
        public async Task<IActionResult> OnGetProcessRawInput([FromQuery] string rawInput)
        {
            var llm = new ApiIntegrations.LLM.GptApiClientLibrary();

            List<Message> messages = new List<Message>() { new Message()
            {
                role = "user",
                content = String.Format(StringResources.PromptModuleCreateWizard, rawInput)
            }};

            string response = await llm.MakeApiRequest(messages, "m4turbo");
            var module = Helpers.ParseModule(response);

            var userId = HttpContext.Session.GetString("UserId");

            var avatars = PractissApiClientLibrary.GetAvatarsByAuthorIdAsync(userId).Result.ToList();
            await Helpers.CreateDefaultAvatars(userId, avatars);

            this.AvatarOptions = new List<SelectListItem> { };
            foreach (var a in avatars)
            {
                this.AvatarOptions.Add(new SelectListItem { Value = a.Name, Text = a.Name });
            }

            return new JsonResult(new
            {
                moduleName = module.Title,
                moduleDescription = module.Description,
                selectedAvatar = module.Avatar.Name,
                situation = module.Situation,
                evaluation = module.Evaluation
            });
        } 
    }
}
