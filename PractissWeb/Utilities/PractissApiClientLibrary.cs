using CommonTypes;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.Text;

namespace PractissWeb.Utilities
{
    public class PractissApiClientLibrary
    {
        public static string ApiUrl { get; set; }

        #region User

        public static async Task<CommonTypes.User> CreateUserAsync(CommonTypes.User newUser)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.PostAsJsonAsync("user", newUser);
                response.EnsureSuccessStatusCode();

                // Deserialize the updated user from the response body.
                CommonTypes.User user = await response.Content.ReadAsAsync<CommonTypes.User>();
                return user;
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<CommonTypes.User> UpdateUserAsync(CommonTypes.User updatedUser)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.PutAsJsonAsync($"user", updatedUser);
                response.EnsureSuccessStatusCode();

                CommonTypes.User user = await response.Content.ReadAsAsync<CommonTypes.User>();
                return user;
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<CommonTypes.User> GetUserAsync(string id)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync($"user/{id}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Handle the case where the user is not found
                    Console.WriteLine("User not found.");
                    return null;
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<CommonTypes.User>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task DeleteUserAsync(string id)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.DeleteAsync($"user/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return;
            }
        }

        public static async Task<CommonTypes.User> GetUserByEmailAsync(string email)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync($"user/email/{email}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Handle the case where the user is not found
                    Console.WriteLine("User not found.");
                    return null;
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<CommonTypes.User>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<List<CommonTypes.User>> GetAllUsersAsync()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync($"user/getallusers");

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<List<CommonTypes.User>>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<List<CommonTypes.User>> GetAllUsersByDomainAsync(string domain)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync($"user/getallusersbydomain/{domain}");

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<List<CommonTypes.User>>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        #endregion

        #region Modules

        public static async Task<CommonTypes.Module> CreateModuleAsync(CommonTypes.Module module)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.PostAsJsonAsync("module", module);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<CommonTypes.Module>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<CommonTypes.Module> GetModuleAsync(string id)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync($"module/{id}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Handle the case where the module is not found
                    Console.WriteLine("Module not found.");
                    return null;
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<CommonTypes.Module>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<Module> UpdateModuleAsync(Module module)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.PutAsJsonAsync($"module", module);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<Module>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task DeleteModuleAsync(string id)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.DeleteAsync($"module/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return;
            }
        }

        public static async Task<IEnumerable<CommonTypes.Module>> GetModulesByAuthorAsync(string authorId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync($"module/author/{authorId}"); // Adjust endpoint as needed
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<IEnumerable<CommonTypes.Module>>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<IEnumerable<CommonTypes.Module>> SearchForModuleAsync(string moduleName, string authorName)
        {
            try
            {
                var queryParams = new Dictionary<string, string>
          {
              { "moduleName", moduleName },
              { "authorName", authorName }
          };
                var query = QueryHelpers.AddQueryString("module/search", queryParams); // Using Microsoft.AspNetCore.WebUtilities;

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync(query);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<IEnumerable<CommonTypes.Module>>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        #endregion

        #region Avatar

        public static async Task<Avatar> CreateAvatarAsync(Avatar avatar)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.PostAsJsonAsync("avatar", avatar);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<Avatar>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<Avatar> GetAvatarAsync(string id)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync($"avatar/{id}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Handle the case where the avatar is not found
                    Console.WriteLine("Avatar not found.");
                    return null;
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<Avatar>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<Avatar> UpdateAvatarAsync(string id, Avatar avatar)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.PutAsJsonAsync($"avatar", avatar);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<Avatar>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task DeleteAvatarAsync(string id)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.DeleteAsync($"avatar/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return;
            }
        }

        public static async Task<IEnumerable<Avatar>> GetAvatarsByAuthorIdAsync(string authorId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync($"avatar/author/{authorId}"); // Adjust endpoint as needed
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<IEnumerable<Avatar>>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }


        #endregion

        #region ModuleAssignment

        public static async Task<CommonTypes.ModuleAssignment> CreateModuleAssignmentAsync(CommonTypes.ModuleAssignment moduleAssignment)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.PostAsJsonAsync("moduleassignment", moduleAssignment);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<CommonTypes.ModuleAssignment>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<ModuleAssignment> UpdateModuleAssignmentAsync(ModuleAssignment moduleAssignment)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.PutAsJsonAsync($"moduleassignment", moduleAssignment);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<CommonTypes.ModuleAssignment>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task DeleteModuleAssignmentAsync(string id)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.DeleteAsync($"moduleassignment/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return;
            }
        }

        public static async Task<IEnumerable<CommonTypes.ModuleAssignment>> GetModuleAssignmentByLearnerAsync(string learnerId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync($"moduleassignment/learner/{learnerId}");
                response.EnsureSuccessStatusCode();

                IEnumerable<CommonTypes.ModuleAssignment> assignments = await response.Content.ReadAsAsync<IEnumerable<CommonTypes.ModuleAssignment>>();
                return assignments;
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<CommonTypes.ModuleAssignment> GetModuleAssignmentByIdAsync(string moduleAssignmentId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync($"moduleassignment/{moduleAssignmentId}");
                response.EnsureSuccessStatusCode();

                ModuleAssignment assignment = await response.Content.ReadAsAsync<ModuleAssignment>();
                return assignment;
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<CommonTypes.ModuleAssignment> GetModuleAssignmentByCoachModuleLearnerAsync(string coachId, string moduleId, string learnerId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync($"moduleassignment/coach/{coachId}/module/{moduleId}/learner/{learnerId}");
                response.EnsureSuccessStatusCode();

                ModuleAssignment assignment = await response.Content.ReadAsAsync<ModuleAssignment>();
                return assignment;
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<IEnumerable<CommonTypes.ModuleAssignment>> GetModuleAssignmentByCoachAsync(string coachId, string moduleId)
        {
            try
            {
                // Update the URL to include moduleId
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync($"moduleassignment/coach/{coachId}/module/{moduleId}");
                response.EnsureSuccessStatusCode();

                IEnumerable<ModuleAssignment> assignments = await response.Content.ReadAsAsync<IEnumerable<ModuleAssignment>>();
                return assignments;
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }


        #endregion

        #region BookmarkedModule

        public static async Task<BookmarkedModule> CreateBookmarkedModuleAsync(BookmarkedModule bookmarkedModule)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.PostAsJsonAsync("bookmarkedmodule", bookmarkedModule);
                response.EnsureSuccessStatusCode();

                BookmarkedModule result = await response.Content.ReadAsAsync<BookmarkedModule>();
                return result;
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task RemoveBookmarkedModuleAsync(string id)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.DeleteAsync($"bookmarkedmodule/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return;
            }
        }

        public static async Task<IEnumerable<BookmarkedModule>> GetBookmarkedModulesByUserIdAsync(string userId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync($"bookmarkedmodule/{userId}");
                response.EnsureSuccessStatusCode();

                IEnumerable<BookmarkedModule> modules = await response.Content.ReadAsAsync<IEnumerable<BookmarkedModule>>();
                return modules;
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }



        #endregion

        #region Report

        public static async Task CreateReportAsync(Report report)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.PostAsJsonAsync("reports", report);
                response.EnsureSuccessStatusCode();

                await response.Content.ReadAsAsync<Report>();
                return;
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return;
            }
        }

        public static async Task<IEnumerable<Report>> SearchReportsByLearnerOrCoachIdAsync(string learnerId, string coachId)
        {
            try
            {
                // Construct the request body
                var requestBody = new
                {
                    LearnerId = learnerId,
                    CoachId = coachId
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiUrl);

                    // Make the request
                    var response = await client.PostAsync("report/search", content);
                    if (response.IsSuccessStatusCode)
                    {
                        // Assuming you deserialize the response content to a list of Report objects
                        var reports = await response.Content.ReadAsAsync<IEnumerable<Report>>();
                        return reports;
                    }
                    else
                    {
                        // Handle non-success status codes appropriately
                        throw new Exception($"API call failed with status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                throw;
            }
        }

        public static async Task<CommonTypes.Report> GetReportByIdAsync(string reportId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync($"report/{reportId}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Handle the case where the report is not found
                    Console.WriteLine("Report not found.");
                    return null;
                }

                response.EnsureSuccessStatusCode();
                var report = await response.Content.ReadAsAsync<CommonTypes.Report>();
                return report;
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<Report> UpdateReportAsync(Report report)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.PutAsJsonAsync($"report", report);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<Report>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<Report> RegenerateReportAsync(string id)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync($"report/{id}/regeneratereport");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<Report>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        #endregion

        #region CoachClientRelationships

        public static async Task<CoachClientRelationship> CreateClientAsync(CoachClientRelationship relationship)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.PostAsJsonAsync("coachclientrelationship", relationship);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<CoachClientRelationship>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<IEnumerable<CoachClientRelationship>> GetClientsAsync(string coachId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync($"coachclientrelationship/{coachId}"); // Adjust endpoint as needed
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<IEnumerable<CoachClientRelationship>>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }


        public static async Task DeleteClientAsync(string relationshipId, string coachId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.DeleteAsync($"coachclientrelationship/coach/{coachId}/relationship/{relationshipId}");
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return;
            }
        }

        #endregion

        #region InteractionWorkflow

        public static async Task<string> GetNextResponseAsync(string moduleAssignmentId, string userResponse)
        {
            var url = $"interaction/{moduleAssignmentId}/getnextresponse";

            if (String.IsNullOrWhiteSpace(userResponse))
                userResponse = "empty";

            // Serialize the userResponse directly as a JSON string
            var json = System.Text.Json.JsonSerializer.Serialize(userResponse);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                var response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
                return null;
            }
        }

        public static async Task<Stream> GetNextResponseStreamAsync(string moduleAssignmentId, string userResponse)
        {
            var url = $"interaction/{moduleAssignmentId}/getnextresponsestream";

            if (String.IsNullOrWhiteSpace(userResponse))
                userResponse = "empty";

            var json = System.Text.Json.JsonSerializer.Serialize(userResponse); // Ensure correct serialization
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpClient client = new HttpClient();
                // Assuming apiUrl is defined and accessible within this context
                client.BaseAddress = new Uri(ApiUrl);

                // Make the POST request and get the response stream
                var response = await client.PostAsync(url, content);
                var inputStream = await response.Content.ReadAsStreamAsync();

                // Initialize a MemoryStream to be used as the output stream
                var outputStream = new MemoryStream();

                // Assuming Helpers.StreamDataWithTimeoutAsync is accessible and correctly defined to take an input and output stream.
                // You need to pass the inputStream from the response to this method.
                await Helpers.StreamDataWithTimeoutAsync(inputStream, outputStream);

                // Reset the MemoryStream position to the beginning for further reading
                outputStream.Seek(0, SeekOrigin.Begin);

                return outputStream;
            }
            catch (Exception e)
            {
                Logger.LogError($"An error occurred: {e.Message}");
                return null;
            }
        }

        public static async Task<string> WrapupInteractionAsync(string moduleAssignmentId)
        {
            var url = $"interaction/{moduleAssignmentId}/wrapupinteraction";

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                var response = await client.PostAsync(url, null);
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
                return null;
            }
        }

        public static async Task AddInteractionReactionAsync(string moduleAssignmentId, Reaction reaction)
        {
            var url = $"interaction/{moduleAssignmentId}/addinteractionreaction";

            var json = System.Text.Json.JsonSerializer.Serialize(reaction); // Ensure correct serialization
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpClient client = new HttpClient();
                // Assuming apiUrl is defined and accessible within this context
                client.BaseAddress = new Uri(ApiUrl);

                // Make the POST request and get the response stream
                var response = await client.PostAsync(url, content);

                return;
            }
            catch (Exception e)
            {
                Logger.LogError($"An error occurred: {e.Message}");
                return;
            }
        }

        #endregion

        #region Insights

        public static async Task CreateRecommendationAsync(Insight insight)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.PostAsJsonAsync("insight", insight);
                response.EnsureSuccessStatusCode();

                await response.Content.ReadAsAsync<Insight>();
                return;
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return;
            }
        }

        public static async Task<IEnumerable<Insight>> GetInsightsAsync(string userId)
        {
            try
            {
                var url = $"insight/{userId}";

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                IEnumerable<Insight> insights = await response.Content.ReadAsAsync<IEnumerable<Insight>>();
                return insights;
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<Insight> GetInsightByIdAsync(string userId, string insightId)
        {
            try
            {
                var url = $"insight/user/{userId}/insight/{insightId}";

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                Insight insight = await response.Content.ReadAsAsync<Insight>();
                return insight;
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        public static async Task<Insight> UpdateInsightAsync(Insight insight)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ApiUrl);

                HttpResponseMessage response = await client.PutAsJsonAsync($"insight", insight);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<Insight>();
            }
            catch (HttpRequestException e)
            {
                // Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                // Handle other unanticipated exceptions
                Console.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

		public static async Task DeleteInsightAsync(string userId, string insightId)
		{
			try
			{
				HttpClient client = new HttpClient();
				client.BaseAddress = new Uri(ApiUrl);

				HttpResponseMessage response = await client.DeleteAsync($"insight/user/{userId}/insight/{insightId}");
				response.EnsureSuccessStatusCode();
			}
			catch (HttpRequestException e)
			{
				// Handle HttpRequestException (e.g., network errors, server unreachable, etc.)
				Console.WriteLine($"HttpRequestException: {e.Message}");
				return;
			}
			catch (Exception e)
			{
				// Handle other unanticipated exceptions
				Console.WriteLine($"Exception: {e.Message}");
				return;
			}
		}

		#endregion
	}
}