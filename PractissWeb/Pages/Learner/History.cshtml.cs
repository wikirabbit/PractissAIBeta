using CommonTypes;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Learner
{
    public class HistoryModel : BasePageModel
    {
        public IEnumerable<Report> History { get; set; }

        public HistoryModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task OnGet()
        {
            var userId = HttpContext.Session.GetString("UserId");
            History = await PractissApiClientLibrary.SearchReportsByLearnerOrCoachIdAsync(userId, null);

            foreach (var report in History)
            {
                report.ModuleAssignment.Coach.ProfileImage = await Helpers.GetProfileImage(report.ModuleAssignment.Coach.Id);
            }
        }
    }
}
