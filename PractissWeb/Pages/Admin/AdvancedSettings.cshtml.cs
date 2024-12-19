using Microsoft.AspNetCore.Mvc;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Admin
{
    public class AdvancedSettingsModel : BasePageModel
    {
        [BindProperty]
        public string AdditionalInstructionsForDialogPrompt { get; set; }
        [BindProperty]
        public string AdditionalInstructionsforEvaluationPrompt { get; set; }
        [BindProperty]
        public string CustomField { get; set; }

        public AdvancedSettingsModel(IWebHostEnvironment webHostEnvironment)
    : base(webHostEnvironment)
        {
        }

        public async Task OnGet()
        {
            var email = HttpContext.Session.GetString("Email");
            var user = await PractissApiClientLibrary.GetUserByEmailAsync(email);

            if (user.AdminPrompts != null)
            {
                AdditionalInstructionsForDialogPrompt = user.AdminPrompts.AdditionalInstructionsForResponsePrompt;
                AdditionalInstructionsforEvaluationPrompt = user.AdminPrompts.AdditionalInstructionsForEvaluationPrompt;
                CustomField = user.AdminPrompts.CustomField;
            }
        }

        public async Task<IActionResult> OnPost()
        {
            //if (!ModelState.IsValid)
            //{
            //  return Page();
            //}

            var email = HttpContext.Session.GetString("Email");
            var user = await PractissApiClientLibrary.GetUserByEmailAsync(email);

            if (user.AdminPrompts == null)
                user.AdminPrompts = new CommonTypes.Admin();

            user.AdminPrompts.AdditionalInstructionsForResponsePrompt = AdditionalInstructionsForDialogPrompt;
            user.AdminPrompts.AdditionalInstructionsForEvaluationPrompt = AdditionalInstructionsforEvaluationPrompt;
            user.AdminPrompts.CustomField = CustomField;

            PractissApiClientLibrary.UpdateUserAsync(user).Wait();

            return RedirectToPage("/Coach/ModuleLibrary"); // Redirect to a success page or any other page
        }
    }
}
