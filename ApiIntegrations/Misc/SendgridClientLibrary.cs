using DataAccess;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ApiIntegrations.Misc
{
	public class SendgridClientLibrary
	{
		public static async Task SendEmail(string toAddress, string subject, string htmlBody)
		{
			if (htmlBody.Length < 10)
				return;

			if (toAddress.Contains("@practiss.ai") && subject.Contains("verification code"))
				return;

			var apiKey = Environment.GetEnvironmentVariable("SendgridApiKey");
			var client = new SendGridClient(apiKey);
			var from = new EmailAddress("info@practiss.ai", "Practiss");
			var to = new EmailAddress(toAddress);
			var plainTextContent = String.Empty;
			var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlBody);
			var response = await client.SendEmailAsync(msg);
		}

		public static async Task SendEmailToPractissTeam(CommonTypes.User user, string action, string details)
		{
			if (user.Email.ToLower().Contains("@practiss.ai"))
				return;

			await SendEmail("info@practiss.ai",
									  string.Format("Whitelist user activity: {0} {1} - {2}", user.FirstName, user.LastName, action),
									  string.Format(StringResources.WhitelistUserActionNotification, user.FirstName, user.LastName, user.Email, action, details));
		}

		public static async Task SendEmailToPractissTeam(string action, string details)
		{
			await SendEmail("info@practiss.ai",
									  string.Format("Non whitelist user activity: {0} - {1}", action, details),
									  string.Format("Non whitelist user activity: {0} - {1}", action, details));
		}

		public static async Task SendErrorAlert(string message)
		{
			// Send an email alert
			SendgridClientLibrary.SendEmail("vijay@practiss.ai",
				"Internal Server Error",
				message);
		}

		public static async Task SendErrorAlert(string message, string userId)
		{
			var user = await CosmosDbService.Instance.GetUserAsync(userId);

			// Send an email alert
			SendgridClientLibrary.SendEmail("vijay@practiss.ai",
				"Internal Server Error for " + (user != null ? (user.FirstName + " " + user.LastName) : ""),
				message);
		}

	}
}
