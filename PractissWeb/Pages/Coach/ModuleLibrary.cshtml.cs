using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Coach
{
    public class ModuleLibraryModel : BasePageModel
    {
        public IEnumerable<Module> Modules { get; set; }

        public IEnumerable<BookmarkedModule> BookmarkedModules { get; set; }

        public string CoachId { get; set; }

        public string Mode { get; set; }

        [BindProperty]
        public string SearchText { get; set; }

        public ModuleLibraryModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task OnGet()
        {
            CoachId = HttpContext.Session.GetString("UserId");
            var coach = await PractissApiClientLibrary.GetUserAsync(CoachId);

            if (coach.StreamingMode)
            {
                Mode = "InteractionStream";
            }
            else
            {
                Mode = "Interaction";
            }

            Modules = await PractissApiClientLibrary.GetModulesByAuthorAsync(CoachId);
            BookmarkedModules = await PractissApiClientLibrary.GetBookmarkedModulesByUserIdAsync(CoachId);
        }
        public async Task<IActionResult> OnPostCopyAsync(string moduleId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var module = await PractissApiClientLibrary.GetModuleAsync(moduleId);
            var newModule = new Module()
            {
                Id = Guid.NewGuid().ToString(),
                Title = module.Title + " Copy",
                Description = module.Description,
                Author = module.Author,
                Avatar = module.Avatar,
                Evaluation = module.Evaluation,
                Visibility = "Private"
            };
            await PractissApiClientLibrary.CreateModuleAsync(newModule);
            return RedirectToPage();
        }
		public async Task<IActionResult> OnPostDeleteModuleAsync(string moduleId)
		{
			await PractissApiClientLibrary.DeleteModuleAsync(moduleId);
			return new JsonResult(new { success = true });
		}
	}
}
