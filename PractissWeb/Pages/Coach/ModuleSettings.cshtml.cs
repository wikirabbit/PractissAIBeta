using ApiIntegrations.Misc;
using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Coach
{
    public class ModuleSettingsModel : BasePageModel
    {
        [BindProperty]
        public string EmailToAdd { get; set; }

        public IEnumerable<ModuleAssignment> AssignedLearners { get; set; }

        [BindProperty]
        public List<User> AvailableLearners { get; set; }

        public string CoachId { get; set; }

        public string ModuleId { get; set; }

        public string ModuleTitle { get; set; }

        [BindProperty]
        public int SessionsAllowed { get; set; }

        public ModuleSettingsModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task OnGet(string moduleId)
        {
            ModuleId = moduleId;
            CoachId = HttpContext.Session.GetString("UserId");
            var coach = await PractissApiClientLibrary.GetUserAsync(CoachId);

            var module = await PractissApiClientLibrary.GetModuleAsync(moduleId);
            ModuleTitle = module.Title;

            // Fetch the list of users for the module
            AssignedLearners = PractissApiClientLibrary.GetModuleAssignmentByCoachAsync(CoachId, ModuleId).Result.ToList();

            var allLearners = await PractissApiClientLibrary.GetClientsAsync(CoachId);
            allLearners = allLearners.OrderBy(x => x.Learner.FirstName);
            var idsToRemove = AssignedLearners.Select(a => a.Learner.Id).ToList();
            allLearners.ToList().RemoveAll(l => idsToRemove.Contains(l.Learner.Id));
            AvailableLearners = allLearners.Select(l => l.Learner).ToList();

            if (AvailableLearners.Count > 1)
            {
                User user = new User()
                {
                    FirstName = "All",
                    LastName = "Learners",
                    Email = "all"
                };

                AvailableLearners.Insert(0, user);
            }
        }

        // Add Learner
        public async Task<IActionResult> OnPost(string moduleId)
        {
            if (!string.IsNullOrWhiteSpace(EmailToAdd))
            {
                CoachId = HttpContext.Session.GetString("UserId");

                var coach = await PractissApiClientLibrary.GetUserAsync(CoachId);
                var module = await PractissApiClientLibrary.GetModuleAsync(moduleId);

                if (EmailToAdd != "all")
                {
                    var moduleAssigment = new ModuleAssignment();
                    moduleAssigment.Id = Guid.NewGuid().ToString();
                    moduleAssigment.Coach = coach;
                    moduleAssigment.Learner = await PractissApiClientLibrary.GetUserByEmailAsync(EmailToAdd);
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
                else
                {
                    // add all available learners

                    // Fetch the list of users for the module
                    AssignedLearners = await PractissApiClientLibrary.GetModuleAssignmentByCoachAsync(CoachId, moduleId);

                    var allLearners = PractissApiClientLibrary.GetClientsAsync(CoachId).Result.ToList();
                    var idsToRemove = AssignedLearners.Select(a => a.Learner.Id).ToList();
                    allLearners.RemoveAll(l => idsToRemove.Contains(l.Learner.Id));
                    AvailableLearners = allLearners.Select(l => l.Learner).ToList();

                    Parallel.ForEach(AvailableLearners, (learner) =>
                    {
                        var moduleAssignment = new ModuleAssignment()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Coach = coach,
                            Learner = learner,
                            Module = module
                        };

                        // Send email
                        var subjectTemplate = TemplateService.Instance.GetTemplate("ModuleAssignedEmailSubjectTemplate").Result;
                        var bodyTemplate = TemplateService.Instance.GetTemplate("ModuleAssignedEmailBodyTemplate").Result;

                        SendgridClientLibrary.SendEmail(learner.Email,
                                    string.Format(subjectTemplate, learner.FirstName, learner.LastName, coach.FirstName, coach.LastName, module.Title),
                                    string.Format(bodyTemplate, learner.FirstName, learner.LastName, coach.FirstName, coach.LastName, module.Title)).Wait();

                        PractissApiClientLibrary.CreateModuleAssignmentAsync(moduleAssignment).Wait();
                    });
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveLearnerAsync(string coachId, string moduleId, string learnerId)
        {
            var assignment = PractissApiClientLibrary.GetModuleAssignmentByCoachModuleLearnerAsync(coachId, moduleId, learnerId).Result;
            await PractissApiClientLibrary.DeleteModuleAssignmentAsync(assignment.Id);
            return new JsonResult(new { success = true });
        }
    }
}
