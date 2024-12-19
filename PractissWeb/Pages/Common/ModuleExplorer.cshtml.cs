using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Common
{
    public class BookmarkActionModel
    {
        public string UserId { get; set; }
        public string ModuleId { get; set; }
    }

    public class ModuleExplorerModel : BasePageModel
    {
        public IEnumerable<CommonTypes.Module> Modules { get; set; }
        [BindProperty]
        public List<BookmarkedModule> BookmarkedModules { get; set; }
        [BindProperty]
        public List<string> BookmarkedModuleIds { get; set; }
        public string UserId { get; set; }

        [BindProperty]
        public string AuthorName { get; set; }
        [BindProperty]
        public string ModuleName { get; set; }
        [BindProperty]
        public string Mode { get; set; }

        public ModuleExplorerModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task OnGet()
        {
            Modules = await PractissApiClientLibrary.SearchForModuleAsync("empty", "empty");

            // TODO - During Beta, show only Joe's modules
            Modules = Modules.Where(m => m.Author.Email == "joe@practiss.ai");

            foreach (var module in Modules)
            {
                if (module.Author.ProfileImage == null)
                {
                    module.Author.ProfileImage = Helpers.GetBase64StringForImage("/img/avatars/reference-blank.png");
                }
            }

            UserId = HttpContext.Session.GetString("UserId");
            var user = await PractissApiClientLibrary.GetUserAsync(UserId);

            if (user.StreamingMode)
            {
                Mode = "InteractionStream";
            }
            else
            {
                Mode = "Interaction";
            }

            BookmarkedModules = PractissApiClientLibrary.GetBookmarkedModulesByUserIdAsync(UserId).Result.ToList();
            BookmarkedModuleIds = BookmarkedModules.Select(x => x.Module.Id).ToList();
        }

        public async Task OnPostAsync()
        {
            ModuleName = ModuleName ?? "empty";
            AuthorName = AuthorName ?? "empty";

            Modules = await PractissApiClientLibrary.SearchForModuleAsync(ModuleName, AuthorName);
        }

        public void CreateModuleBookmark(string userId, string moduleId)
        {
            var bookmark = new BookmarkedModule()
            {
                Id = Guid.NewGuid().ToString(),
                Module = new CommonTypes.Module() { Id = moduleId },
                User = new CommonTypes.User() { Id = userId }
            };

            BookmarkedModules.Add(bookmark);

            PractissApiClientLibrary.CreateBookmarkedModuleAsync(bookmark).Wait();
        }

        public void RemoveModuleBookmark(string userId, string moduleId)
        {
            BookmarkedModules = PractissApiClientLibrary.GetBookmarkedModulesByUserIdAsync(userId).Result.ToList();

            BookmarkedModule bookmark = BookmarkedModules.Where(x => x.User.Id == userId && x.Module.Id == moduleId).FirstOrDefault();
            BookmarkedModules.Remove(bookmark);

            PractissApiClientLibrary.RemoveBookmarkedModuleAsync(bookmark.Id).Wait();
        }

        public async Task<IActionResult> OnPostAddBookmarkAsync([FromBody] BookmarkActionModel model)
        {
            CreateModuleBookmark(model.UserId, model.ModuleId);
            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostRemoveBookmarkAsync([FromBody] BookmarkActionModel model)
        {
            RemoveModuleBookmark(model.UserId, model.ModuleId);
            return new JsonResult(new { success = true });
        }
    }
}
