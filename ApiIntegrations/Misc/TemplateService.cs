using DataAccess;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System.Text.RegularExpressions;

namespace ApiIntegrations.Misc
{
	public class TemplateService
    {
        private SheetsService _sheetsService;
        private string _jsonKey;
        private string _spreadsheetId;
        private static TemplateService _instance;

        private TemplateService()
        {
            try
            {
                _jsonKey = StringResources.GoogleSpreadsheetAccessApiKeyJson;
                _spreadsheetId = StringResources.TemplateSpreadsheetId;

                // Parse the JSON credentials string
                GoogleCredential credential = GoogleCredential.FromJson(_jsonKey)
                    .CreateScoped(SheetsService.Scope.Spreadsheets);

                _sheetsService = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Template Application",
                });

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

        public static TemplateService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TemplateService();
                }
                return _instance;
            }
        }

        public TemplateService(string credentialsJson, string spreadsheetId)
        {
            // Parse the JSON credentials string
            GoogleCredential credential = GoogleCredential.FromJson(credentialsJson)
                .CreateScoped(SheetsService.Scope.Spreadsheets);

            _sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Whitelist Application",
            });

            _spreadsheetId = spreadsheetId;
        }


        public async Task<string> GetTemplate(string templateId)
        {
            var spreadsheet = _sheetsService.Spreadsheets.Get(_spreadsheetId).Execute();

            foreach (var sheet in spreadsheet.Sheets)
            {
                var range = $"{sheet.Properties.Title}!A:B";
                var response = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range).Execute();
                if (response.Values != null)
                {
                    foreach (var row in response.Values)
                    {
                        if (row.Count > 0 && string.Equals(row[0].ToString(), templateId, StringComparison.OrdinalIgnoreCase))
                        {
                            var originalString = row[1].ToString();

							// Step 1: Escape all braces.
							string escapedString = originalString.Replace("{", "{{").Replace("}", "}}");

							// Step 2: Use regex to unescape specific placeholders.
							// This regex finds patterns like {{[0-9]+}} which corresponds to our doubled placeholders
							escapedString = Regex.Replace(escapedString, @"\{\{(\d+)\}\}", @"{$1}");

							return escapedString;
                        }
                    }
                }
            }

            return null; // Return null if the email is not found in the spreadsheet
        }
    }
}