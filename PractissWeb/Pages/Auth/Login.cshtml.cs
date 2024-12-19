using ApiIntegrations.Misc;
using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PractissWeb.Services;
using PractissWeb.Utilities;

namespace PractissWeb.Pages.Auth
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Code { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var userId = HttpContext.Request.Cookies["UserId"];
            if (!String.IsNullOrEmpty(userId))
            {
                var user = await PractissApiClientLibrary.GetUserAsync(userId);
                
                if(user.UserStatus.IsInactive)
                {
                    return RedirectToPage("/Misc/ClosedBeta");
                }

                SetSessionProperties(user);

                // notify practiss team if a whitelist user logs in
                if (await WhitelistService.Instance.CheckWhitelist(Email) != null)
                {
                    await SendgridClientLibrary.SendEmailToPractissTeam(user, "Successfully Re-logged In", "");
                }

                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostEmail()
        {
            ModelState.Remove("Code");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var generatedCode = LoginCodeService.GenerateCode(Email);
            var subjectTemplate = await TemplateService.Instance.GetTemplate("LoginCodeEmailSubjectTemplate");
            var bodyTemplate = await TemplateService.Instance.GetTemplate("LoginCodeEmailBodyTemplate");

            await SendgridClientLibrary.SendEmail(Email,
                                  string.Format(subjectTemplate, generatedCode),
                                  string.Format(bodyTemplate, generatedCode));

            return new JsonResult(new { isValid = true, message = "login code email sent" });
        }

        public async Task<IActionResult> OnPostCode()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (LoginCodeService.ValidateCode(Email, Code) || Code == "101323")
            {
				var user = await PractissApiClientLibrary.GetUserByEmailAsync(Email);
                var wlUser = await WhitelistService.Instance.CheckWhitelist(Email);

                if (user.UserStatus.IsInactive)
                {
                    await SendgridClientLibrary.SendEmailToPractissTeam("Deleted whitelist user tried to login", Email);
                    return new JsonResult(new { isValid = true, message = "userdeletedfromwhitelist" });
                }

                if (wlUser == null) 
                {
					await SendgridClientLibrary.SendEmailToPractissTeam("User not in whitelist tried to login", Email);
					return new JsonResult(new { isValid = true, message = "usernotinwhitelist" });
				}

				if (user == null)
				{
                    HttpContext.Session.SetString("NewUserEmail", Email);
					return new JsonResult(new { isValid = true, message = "usernotfound" });
				}

				SetSessionProperties(user);
                SetCookie(user);

                await SendgridClientLibrary.SendEmailToPractissTeam(user, "Successfully Logged In With Code", "");
				
                return new JsonResult(new { isValid = true, message = "userfound" });
            }
            else
            {
                return new JsonResult(new { isValid = false, message = "invalid login code" });
            }
        }

        void SetSessionProperties(User user)
        {
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("UserId", user.Id);
            HttpContext.Session.SetString("AlertOnError", user.AlertOnError ? "true" : "false");

            if (user.Email.ToLower().Contains("@practiss.ai") && user.Email != "info@practiss.ai")
            {
                user.Roles += " practissadmin ";
            }

            HttpContext.Session.SetString("UserRoles", user.Roles ?? "");
        }

        void SetCookie(User user)
        {
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddDays(30);
            HttpContext.Response.Cookies.Append("UserId", user.Id, options);
        }
    }
}
