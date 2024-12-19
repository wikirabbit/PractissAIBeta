using CommonTypes;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Coach
{
    public class ReportsSearchModel : BasePageModel
    {
        public IEnumerable<Report> Reports { get; set; }

        public ReportsSearchModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task OnGet()
        {
            var userId = HttpContext.Session.GetString("UserId");
            Reports = await PractissApiClientLibrary.SearchReportsByLearnerOrCoachIdAsync(null, userId);

            foreach (var report in Reports)
            {
                report.ModuleAssignment.Learner.ProfileImage = await Helpers.GetProfileImage(report.ModuleAssignment.Learner.Id);
            }
        }
    }
}
