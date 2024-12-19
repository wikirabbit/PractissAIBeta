using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Misc
{
    public class ProductTourModel : PageModel
    {
        public string NextPage { get; set; }
        public async Task OnGet()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var moduleAssignments = await PractissApiClientLibrary.GetModuleAssignmentByLearnerAsync(userId);

            if(moduleAssignments == null || moduleAssignments.Count() == 0)
            {
                NextPage = "/Common/ModuleExplorer/";
            }
            else
            {
                NextPage = "/Learner/Modules/";
            }
        }
    }
}
