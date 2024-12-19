using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Learner
{
    public class InsightDetailsModel : BasePageModel
    {
        [BindProperty]
        public Insight Insight { get; set; }

        public int OverallScore { get; set; }

        public string UserId { get; set; }
        public InsightDetailsModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }


        public async Task OnGet(string insightId)
        {
            UserId = HttpContext.Session.GetString("UserId");
            Insight = await PractissApiClientLibrary.GetInsightByIdAsync(UserId, insightId);
            OverallScore = Helpers.CalculateFinalScoreV3(Insight.QuantitativeFeedbackV3);
        }

        public async Task<IActionResult> OnPostStartRoleplay(string insightId)
        {
            UserId = HttpContext.Session.GetString("UserId");
            Insight = await PractissApiClientLibrary.GetInsightByIdAsync(UserId, insightId);

            var learner = await PractissApiClientLibrary.GetUserAsync(UserId);
            var coach = await PractissApiClientLibrary.GetUserByEmailAsync("info@practiss.ai");
            var avatar = await PractissApiClientLibrary.GetAvatarAsync("318b43fa-3d99-40c2-9e39-dc3af4f217af");

            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            var jsonData = JObject.Parse(requestBody);

            // Assuming your JSON key in the AJAX call is "roleplayDetailsText"
            var roleplayDetailsText = jsonData["roleplayDetails"].ToString();
            roleplayDetailsText = roleplayDetailsText.Replace("                    Scenario:", "**Scenario**:");
			roleplayDetailsText = roleplayDetailsText.Replace("                    Objective:", "**Objective**:");
            roleplayDetailsText = roleplayDetailsText.Replace("\t", "");

            var module = new Module()
            {
                Id = Guid.NewGuid().ToString(),
                Visibility = "private",
                Avatar = avatar,
                Description = roleplayDetailsText,
                Situation = roleplayDetailsText,
                Title = Insight.Source + " : " + Insight.Title,
                Author = coach,
                Evaluation = "Did [Learner] achieve the roleplay objective?",
			    _ttl = 3600 // Auto delete this transient module after 1 hour
			};

            await PractissApiClientLibrary.CreateModuleAsync(module);

            var moduleAssignment = new ModuleAssignment()
            {
                Id = Guid.NewGuid().ToString(),
                Coach = coach,
                Learner = learner,
                Hidden = true,
                Module = module,
                _ttl = 3600 // Auto delete this transient module after 1 hour
            };

            await PractissApiClientLibrary.CreateModuleAssignmentAsync(moduleAssignment);
            
            // Print to the console
            Console.WriteLine(moduleAssignment.Id);

            return new JsonResult(new { isValid = true, moduleAssignmentId = moduleAssignment.Id });
        }
    }
}
