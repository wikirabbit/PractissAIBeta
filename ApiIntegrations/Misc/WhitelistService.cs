using CommonTypes;
using DataAccess;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace ApiIntegrations.Misc
{
  public class WhitelistService
  {
    private SheetsService _sheetsService;
    private string _jsonKey;
    private string _spreadsheetId;
    private static WhitelistService _instance;

    private WhitelistService()
    {
      try
      {
        _jsonKey = StringResources.GoogleSpreadsheetAccessApiKeyJson;
        _spreadsheetId = StringResources.WhitelistSpreadsheetId;

        // Parse the JSON credentials string
        GoogleCredential credential = GoogleCredential.FromJson(_jsonKey)
            .CreateScoped(SheetsService.Scope.Spreadsheets);

        _sheetsService = new SheetsService(new BaseClientService.Initializer()
        {
          HttpClientInitializer = credential,
          ApplicationName = "Whitelist Application",
        });

      }
      catch (Exception ex)
      {
        Logger.LogError(ex.Message);
      }
    }

    public static WhitelistService Instance
    {
      get
      {
        if (_instance == null)
        {
          _instance = new WhitelistService();
        }
        return _instance;
      }
    }

    public WhitelistService(string credentialsJson, string spreadsheetId)
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


    public async Task<WhitelistUser> CheckWhitelist(string email)
    {
      var spreadsheet = _sheetsService.Spreadsheets.Get(_spreadsheetId).Execute();
      foreach (var sheet in spreadsheet.Sheets)
      {
        var range = $"{sheet.Properties.Title}!A:D"; // Adjust the range to include all columns A through D
        var response = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range).Execute();
        if (response.Values != null)
        {
          foreach (var row in response.Values)
          {
            // Check if the first column (Email) matches the provided email
            if (row.Count > 0 && string.Equals(row[0].ToString(), email, StringComparison.OrdinalIgnoreCase))
            {
              // not an email row
              if (!row[0].ToString().Contains("@"))
                continue;

              // Construct a WhitelistUser object with the row's details
              return new WhitelistUser
              {
                Email = row[0].ToString(),
                Name = row.Count > 1 ? row[1].ToString() : string.Empty,
                CompanyName = row.Count > 2 ? row[2].ToString() : string.Empty,
                MaximumClients = row.Count > 3 ? int.Parse(row[3].ToString()) : 0,
                GroupName = sheet.Properties.Title
              };
            }
          }
        }
      }
      return null; // Return null if the email is not found in the spreadsheet
    }

    public async Task<List<WhitelistUser>> GetAllWhitelistUsers()
    {
      List<WhitelistUser> users = new List<WhitelistUser>();
      var spreadsheet = _sheetsService.Spreadsheets.Get(_spreadsheetId).Execute();
      foreach (var sheet in spreadsheet.Sheets)
      {
        var range = $"{sheet.Properties.Title}!A:D"; // Adjusted to read columns A to D
        var response = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range).Execute();
        if (response.Values != null)
        {
          foreach (var row in response.Values)
          {
            if (row.Count >= 4 && !string.IsNullOrWhiteSpace(row[0]?.ToString())) // Ensure row has at least 4 columns
            {
              // not an email row
              if (!row[0].ToString().Contains("@"))
                continue;

              users.Add(new WhitelistUser
              {
                Email = row[0].ToString(),
                Name = row[1].ToString(),
                CompanyName = row[2]?.ToString(), // Using null-conditional operator for optional fields
                MaximumClients = int.TryParse(row[3]?.ToString(), out int maxClients) ? maxClients : 0, // Parse MaximumClients safely
				GroupName = sheet.Properties.Title
			  });
            }
          }
        }
      }
      return users;
    }

    private async Task<int> FindRowByEmail(string sheetName, string email)
    {
      var range = $"{sheetName}!A:A"; // Assuming emails are in column A
      var response = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range).Execute();
      if (response.Values != null)
      {
        for (int i = 0; i < response.Values.Count; i++)
        {
          if (response.Values[i].Count > 0 && response.Values[i][0].ToString().Equals(email, StringComparison.OrdinalIgnoreCase))
          {
            return i + 1; // +1 because Sheets rows are 1-indexed
          }
        }
      }
      return -1; // Indicates not found
    }

    public async Task UpdateUserStatsInSpreadsheet(string sheetName, string email, UserStatsForReporting userStats)
    {
      int row = await FindRowByEmail(sheetName, email);
      if (row == -1)
      {
        Console.WriteLine("Email not found in the spreadsheet.");
        return;
      }

      List<ValueRange> data = new List<ValueRange>();
      var rangeToUpdate = $"{sheetName}!G{row}:L{row}";
      data.Add(new ValueRange
      {
        Range = rangeToUpdate,
        Values = new List<IList<object>> { new List<object> {
            userStats.TimeOnPlatform,
            userStats.SessionsCompleted,
            userStats.SessionsAborted,
            userStats.CommentsProvided,
            userStats.ReactionsProvided,
            userStats.DaysInactive
        }}
      });

      BatchUpdateValuesRequest body = new BatchUpdateValuesRequest
      {
        ValueInputOption = "RAW",
        Data = data
      };

      var updateRequest = _sheetsService.Spreadsheets.Values.BatchUpdate(body, _spreadsheetId);
      var response = updateRequest.Execute();

      Console.WriteLine($"Updated {response.TotalUpdatedCells} cells.");
    }
  }
}
