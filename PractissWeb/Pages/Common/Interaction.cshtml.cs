using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Common
{
    public class InteractionModel : BasePageModel
    {
        public ModuleAssignment ModuleAssignment { get; set; }

        public InteractionModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task<IActionResult> OnGet(string moduleAssignmentId)
        {
            // This is when you do an "Inspect" on the page
            // and for some reason, the cognitive services
            // sdk path pops up in the page url. So just ignore. 
            // Check if the requested file has a .js.map extension
            if (moduleAssignmentId.EndsWith(".js.map"))
            {
                // Set the status code to 403 Forbidden
                HttpContext.Response.StatusCode = 403;

                // Return a ContentResult with an empty string to stop further processing and prevent page rendering
                return new ContentResult { Content = "", ContentType = "text/plain", StatusCode = 403 };

            }

            var moduleAssignment = await PractissApiClientLibrary.GetModuleAssignmentByIdAsync(moduleAssignmentId);

            // Check if the moduleAssignmentId is a moduleId
            // If so, then this call is made by a coach in the demo mode
            // So create a new ModuleAssignment if one doesn't exist

            if (moduleAssignment == null)
            {
                var module = await PractissApiClientLibrary.GetModuleAsync(moduleAssignmentId);

                var coachId = HttpContext.Session.GetString("UserId");
                var coach = await PractissApiClientLibrary.GetUserAsync(coachId);

                var moduleAssigments = await PractissApiClientLibrary.GetModuleAssignmentByCoachAsync(coachId, module.Id);
                foreach (var ma in moduleAssigments)
                {
                    if (ma.Learner.Id == coachId)
                    {
                        // Coach has already done a demo run, so reuse the moduleAssigment
                        moduleAssignment = ma;
                    }
                }

                if (moduleAssignment == null)
                {
                    // First time coach is doing a demo for this module. So create a new ModuleAssignment
                    moduleAssignment = new ModuleAssignment()
                    {
                        Coach = coach,
                        Learner = coach,
                        Id = Guid.NewGuid().ToString(),
                        Module = module,
                        InteractionsAllowed = 100,
                        InteractionsCount = 0,
                        Hidden = true
                    };

                    await PractissApiClientLibrary.CreateModuleAssignmentAsync(moduleAssignment);
                }
            }

            ModuleAssignment = moduleAssignment;

            return Page();
        }
    }
}
