using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Admin
{
    public class AllReportsModel : BasePageModel
    {
        public IEnumerable<Report> AllReports { get; set; }

        [BindProperty]
        public string LearnerEmail { get; set; }

        [BindProperty]
        public string CoachEmail { get; set; }

        public AllReportsModel(IWebHostEnvironment webHostEnvironment)
    : base(webHostEnvironment)
        {
        }

        public async Task OnGet(string learnerEmail = null, string coachEmail = null)
        {
            var learner = await PractissApiClientLibrary.GetUserByEmailAsync(learnerEmail);
            var coach = await PractissApiClientLibrary.GetUserByEmailAsync(coachEmail);

            AllReports = await PractissApiClientLibrary.SearchReportsByLearnerOrCoachIdAsync(learner?.Id, coach?.Id);
        }

        public async Task OnPost()
        {
            var learner = await PractissApiClientLibrary.GetUserByEmailAsync(LearnerEmail);
            var coach = await PractissApiClientLibrary.GetUserByEmailAsync(CoachEmail);

            AllReports = await PractissApiClientLibrary.SearchReportsByLearnerOrCoachIdAsync(learner?.Id, coach?.Id);
            AllReports = AllReports.Where(x => x.ModuleAssignment.Learner.FirstName != "Vijay");
        }
    }
}
