using ApiIntegrations.Misc;
using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Coach
{
    public class ClientSettingsModel : BasePageModel
    {
        [BindProperty]
        public string ModuleToAdd { get; set; }

        public IEnumerable<ModuleAssignment> AssignedModules { get; set; }

        [BindProperty]
        public List<Module> AvailableModules { get; set; }

        public string CoachId { get; set; }

        public string ClientId { get; set; }

        public string ClientName { get; set; }

        public ClientSettingsModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task OnGet(string clientId)
        {
            ClientId = clientId;
            CoachId = HttpContext.Session.GetString("UserId");

            var coach = await PractissApiClientLibrary.GetUserAsync(CoachId);
            var client = await PractissApiClientLibrary.GetUserAsync(ClientId);
            ClientName = client.FirstName + " " + client.LastName;

			// Fetch the list of modules for the client
			AssignedModules = PractissApiClientLibrary.GetModuleAssignmentByLearnerAsync(ClientId).Result.Where(x => x.Coach.Id == CoachId).ToList();

            var allModules = PractissApiClientLibrary.GetModulesByAuthorAsync(CoachId).Result.ToList();
			var bookMarkedModules = await PractissApiClientLibrary.GetBookmarkedModulesByUserIdAsync(CoachId);

            foreach(var module in bookMarkedModules) 
            {
                allModules.Add(module.Module);
            }

            allModules = allModules.OrderBy(x => x.Title).ToList();
            var idsToRemove = AssignedModules.Select(a => a.Module.Title).ToList();

			allModules.RemoveAll(m => idsToRemove.Contains(m.Title));
            AvailableModules = allModules;
        }

        // Add Module
        public async Task<IActionResult> OnPost(string clientId)
        {
            if (!string.IsNullOrWhiteSpace(ModuleToAdd))
            {
                CoachId = HttpContext.Session.GetString("UserId");

                var coach = await PractissApiClientLibrary.GetUserAsync(CoachId);
                var client = await PractissApiClientLibrary.GetUserAsync(clientId);
                var module = await PractissApiClientLibrary.GetModuleAsync(ModuleToAdd);

                var moduleAssigment = new ModuleAssignment();
                moduleAssigment.Id = Guid.NewGuid().ToString();
                moduleAssigment.Coach = coach;
                moduleAssigment.Learner = client;
                moduleAssigment.Module = module;

                // Send email
                var subjectTemplate = TemplateService.Instance.GetTemplate("ModuleAssignedEmailSubjectTemplate").Result;
                var bodyTemplate = TemplateService.Instance.GetTemplate("ModuleAssignedEmailBodyTemplate").Result;

                SendgridClientLibrary.SendEmail(moduleAssigment.Learner.Email,
                                      string.Format(subjectTemplate, moduleAssigment.Learner.FirstName, moduleAssigment.Learner.LastName, coach.FirstName, coach.LastName, module.Title),
                                      string.Format(bodyTemplate, moduleAssigment.Learner.FirstName, moduleAssigment.Learner.LastName, coach.FirstName, coach.LastName, module.Title)).Wait();


                // moduleAssigment.SessionsMax = SessionsAllowed;
                await PractissApiClientLibrary.CreateModuleAssignmentAsync(moduleAssigment);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveModuleAsync(string coachId, string moduleId, string learnerId)
        {
            var assignment = PractissApiClientLibrary.GetModuleAssignmentByCoachModuleLearnerAsync(coachId, moduleId, learnerId).Result;
            await PractissApiClientLibrary.DeleteModuleAssignmentAsync(assignment.Id);
            return new JsonResult(new { success = true });
        }
    }
}
