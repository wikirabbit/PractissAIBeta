using ApiIntegrations.Misc;
using CommonTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using PractissWeb.Utilities;
using System.ComponentModel.DataAnnotations;

namespace PractissWeb.Pages.Coach
{
    public class IndividualsModel : BasePageModel
    {

        [BindProperty]
        [Required(ErrorMessage = "First name is required.")]
        public string FirstNameToAdd { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Last name is required.")]
        public string LastNameToAdd { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Email is required.")]
        public string EmailToAdd { get; set; }

        public IEnumerable<CoachClientRelationship> ClientRelationships { get; set; }

        public string CoachId { get; set; }

        public bool MaxClientsReached { get; set; }

        public IndividualsModel(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment)
        {
        }

        public async Task OnGet()
        {
            CoachId = HttpContext.Session.GetString("UserId");
            ClientRelationships = await PractissApiClientLibrary.GetClientsAsync(CoachId);

            var user = await PractissApiClientLibrary.GetUserAsync(CoachId);
            var whitelistUser = await WhitelistService.Instance.CheckWhitelist(user.Email);

            if (whitelistUser != null && whitelistUser.MaximumClients < ClientRelationships.Count())
            {
                MaxClientsReached = true;
            }
        }

        [HttpGet]
        public async Task<IActionResult> OnGetNameFromEmail([FromQuery] string email)
        {
            var user = await PractissApiClientLibrary.GetUserByEmailAsync(email);
            if (user != null)
            {
                return new JsonResult(new { isUserFound = true, firstName = user.FirstName, lastName = user.LastName });
            }
            else
            {
                return new JsonResult(new { isUserFound = false, firstName = String.Empty, lastName = String.Empty });
            }
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!string.IsNullOrWhiteSpace(EmailToAdd))
            {
                CoachId = HttpContext.Session.GetString("UserId");
                
                // Create a learner if they don't exist in the system
                var learner = await PractissApiClientLibrary.GetUserByEmailAsync(EmailToAdd);
                if (learner == null)
                {
                    learner = new CommonTypes.User()
                    {
                        Id = Guid.NewGuid().ToString(),
                        FirstName = FirstNameToAdd,
                        LastName = LastNameToAdd,
                        Email = EmailToAdd,
                        Roles = "learner"
                    };

                    await PractissApiClientLibrary.CreateUserAsync(learner);
                }

                // create client relationship
                var relationship = new CoachClientRelationship()
                {
                    Id = Guid.NewGuid().ToString(),
                    CoachId = CoachId,
                    Learner = learner
                };

                var coach = await PractissApiClientLibrary.GetUserAsync(CoachId);

                // Send email
                await SendgridClientLibrary.SendEmailToPractissTeam(coach, "Added Client ", learner.FirstName + " " + learner.LastName + " " + learner.Email);

                await PractissApiClientLibrary.CreateClientAsync(relationship);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveLearnerAsync(string coachId, string learnerId)
        {
            var clients = await PractissApiClientLibrary.GetClientsAsync(coachId);
            var client = clients.Where(x => x.Learner.Id == learnerId).First();
            await PractissApiClientLibrary.DeleteClientAsync(client.Id, coachId);
            return new JsonResult(new { success = true });
        }
    }
}
