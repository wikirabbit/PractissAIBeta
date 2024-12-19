using CommonTypes;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Coach
{
    public class AvatarLibraryModel : BasePageModel
    {
        public IEnumerable<Avatar> Avatars { get; set; }

        public AvatarLibraryModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task OnGet()
        {
            var authorId = HttpContext.Session.GetString("UserId");
            Avatars = await PractissApiClientLibrary.GetAvatarsByAuthorIdAsync(authorId);
        }
    }
}
