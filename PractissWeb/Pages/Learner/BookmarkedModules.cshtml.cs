using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Learner
{
    public class BookmarkedModulesModel : BasePageModel
    {
        public IEnumerable<BookmarkedModule> BookmarkedModules { get; set; }

        [BindProperty]
        public string Mode { get; set; }

        public BookmarkedModulesModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task OnGet()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var user = await PractissApiClientLibrary.GetUserAsync(userId);

            if (user.StreamingMode)
            {
                Mode = "InteractionStream";
            }
            else
            {
                Mode = "Interaction";
            }


            BookmarkedModules = await PractissApiClientLibrary.GetBookmarkedModulesByUserIdAsync(userId);
        }
    }
}