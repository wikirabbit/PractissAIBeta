using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Common
{
    public class ReportDetailsModel : BasePageModel
    {
        [BindProperty]
        public Report Report { get; set; }

        public string UserId { get; set; }

        public string Email { get; set; }

        public int OverallScorePercentage { get; set; }
        public int QuestionsAnsweredPercentage { get; set; }

        public ReportDetailsModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task OnGet(string reportId)
        {
            Report = await PractissApiClientLibrary.GetReportByIdAsync(reportId);

            if(Report.Conversation.Count > 4)
            {
                HttpContext.Session.SetString("ShowUsageGuide", "false");
			}

            if (Report.QuantitativeFeedbackV2 != null)
            {
                QuestionsAnsweredPercentage = Helpers.RevalidateScoresV2(Report);
                OverallScorePercentage = Helpers.CalculateFinalScoreV2(Report.QuantitativeFeedbackV2);
            }

            if (Report.QuantitativeFeedbackV3 != null)
            {
                QuestionsAnsweredPercentage = Helpers.RevalidateScoresV3(Report);
                OverallScorePercentage = Helpers.CalculateFinalScoreV3(Report.QuantitativeFeedbackV3);
            }

            UserId = HttpContext.Session.GetString("UserId");
            Email = HttpContext.Session.GetString("Email");
        }

        public async Task<IActionResult> OnPostAsync(string reportId)
        {
            ModelState.Remove("Tag");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var confidenceLevel = Report.SelfReportedConfidenceLevel;
            Report = await PractissApiClientLibrary.GetReportByIdAsync(reportId);
            Report.SelfReportedConfidenceLevel = confidenceLevel;

            await PractissApiClientLibrary.UpdateReportAsync(Report);

            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostTagAsync()
        {
            await PractissApiClientLibrary.UpdateReportAsync(Report);

            return RedirectToPage("/Admin/AllReports");
        }

    }
}
