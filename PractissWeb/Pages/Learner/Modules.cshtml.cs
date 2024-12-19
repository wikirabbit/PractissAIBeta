using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Learner
{
    public class ModulesModel : BasePageModel
    {
        public IEnumerable<ModuleAssignment> ModuleAssignments { get; set; }
        public IEnumerable<BookmarkedModule> BookmarkedModules { get; set; }

        [BindProperty]
        public string Mode { get; set; }

        public ModulesModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task OnGet()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var user = await PractissApiClientLibrary.GetUserAsync(userId);

            if (user.StreamingMode)
            {
                Mode = "InteractionStream";
            }
            else
            {
                Mode = "Interaction";
            }


            ModuleAssignments = await PractissApiClientLibrary.GetModuleAssignmentByLearnerAsync(userId);

            BookmarkedModules = await PractissApiClientLibrary.GetBookmarkedModulesByUserIdAsync(userId);
        }
    }
}