using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Learner
{
    public class CommunicationInsightsModel : BasePageModel
    {
		[BindProperty]
		public IEnumerable<Insight> Insights { get; set; }

        public CommunicationInsightsModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task OnGet()
        {
            var userId = HttpContext.Session.GetString("UserId");
            Insights = await PractissApiClientLibrary.GetInsightsAsync(userId);
        }

		public async Task<IActionResult> OnPostDeleteInsightAsync(string insightId)
		{
			var userId = HttpContext.Session.GetString("UserId");
			await PractissApiClientLibrary.DeleteInsightAsync(userId, insightId);
			return new JsonResult(new { success = true });
		}
	}
}
