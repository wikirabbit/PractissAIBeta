

using ApiIntegrations.LLM;
using CommonTypes;
using DataAccess;
using PractissWeb.Utilities;
using PractissWorkflow;

namespace Test
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			DataAccess.CosmosDbService.RefreshConnectionDynamically = false;
			DataAccess.CosmosDbService.ConnectionString = StringResource.CosmosDbConnectionString;
			DataAccess.CosmosDbService.DatabaseName = StringResource.CosmosDatataseName;

			// await SanitizeAllReports();
			// await CleanupMyTestReports();
			// await SanitizeAllReports();
			// await CheckBackCompat();
			// await RemoveOutdatedInsights();
			// await RegenerateAllReports();
			// await AddBetaWhitelistWave();
			// await ExtractAmeliasReports();
			// await FixDateTimeInReports();
			// await CleanupTestReports();
			// await TestParsingAdditionalQuestions();
			// await TestGenerateV3FeedbackReports();
			// await FixDateTimeInInsights();
			// await GetJoesModules();
			// await TestModuleParse();
			// await CleanupAvatarPersonality();
			// await RegenerateAllReportsForClaude();
			await MakeBetaUsersDesigners();

            //Insight r = new Insight();
            //r.Id = Guid.NewGuid().ToString();
            //r.Source = "Source";
            //r.ReportMarkdown = "Report";
            //r.Title = "Title";
            //r.Date = DateTime.Now.ToShortDateString();
            //r.UserId = Guid.NewGuid().ToString();

            //await CosmosDbService.Instance.CreateInsightAsync(r);

            // await CosmosDbService.Instance.DeleteAllInsightsAsync();
        }

		static async Task CleanupTestReports()
		{
			try
			{
				var reports = await CosmosDbService.Instance.SearchReportsByLearnerOrCoachIdAsync(null, null);
				int testReportsCount = 0;
				foreach (var report in reports)
				{
					var tmpReport = await CosmosDbService.Instance.GetReportByIdAsync(report.Id);
					if (tmpReport.Conversation == null || tmpReport.Conversation.Count < 4)
					{
						testReportsCount++;
						await CosmosDbService.Instance.DeleteReportAsync(tmpReport.Id);
					}
				}

				Console.WriteLine(testReportsCount);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		static async Task CleanupMyTestReports()
		{
			try
			{
				var user = await CosmosDbService.Instance.GetUserByEmail("vijay@practiss.ai");
				var reports = await CosmosDbService.Instance.SearchReportsByLearnerOrCoachIdAsync(user.Id, null);
				int testReportsCount = 0;
				foreach (var report in reports)
				{
					var tmpReport = await CosmosDbService.Instance.GetReportByIdAsync(report.Id);
					if (tmpReport.Conversation == null || tmpReport.Conversation.Count < 4)
					{
						testReportsCount++;
						await CosmosDbService.Instance.DeleteReportAsync(tmpReport.Id);
					}
				}

				Console.WriteLine(testReportsCount);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		static async Task FixSelfassignedModuleVisibility()
		{
			try
			{
				var users = await CosmosDbService.Instance.GetAllUsersAsync();
				foreach (var user in users)
				{
					var moduleAssignments = await CosmosDbService.Instance.GetModuleAssignmentByLearnerAsync(user.Id);

					foreach (var ma in moduleAssignments)
					{
						if (ma.Learner.Id == ma.Coach.Id)
						{
							ma.Hidden = true;
							await CosmosDbService.Instance.UpdateModuleAssignmentAsync(ma.Id, ma);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		static async Task SanitizeAllReports()
		{
			var tmp = await CosmosDbService.Instance.GetReportByIdAsync("6975719d-253e-4739-a7e3-5df9dec99b72");
			tmp.Sanitize();
			await CosmosDbService.Instance.UpdateReportAsync(tmp.Id, tmp);

			try
			{
				var reports = await CosmosDbService.Instance.SearchReportsByLearnerOrCoachIdAsync(null, null);
				int corrupted = 0;
				int alright = 0;

				foreach (var report in reports)
				{
					if (report.Id != "6975719d-253e-4739-a7e3-5df9dec99b72")
						continue;

					tmp = await CosmosDbService.Instance.GetReportByIdAsync(report.Id);
					if (String.IsNullOrEmpty(tmp.Markdown))
					{
						corrupted++;
						await CosmosDbService.Instance.DeleteReportAsync(report.Id);
					}
					else
					{
						alright++;
						tmp.Sanitize();
						await CosmosDbService.Instance.UpdateReportAsync(tmp.Id, tmp);
					}
				}

				Console.WriteLine(corrupted);
				Console.WriteLine(alright);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		static async Task CheckBackCompat()
		{
			var reports = await CosmosDbService.RestoreInstance.SearchReportsByLearnerOrCoachIdAsync(null, null);

			int corrupted = 0;
			int alright = 0;

			foreach (var r in reports)
			{
				try
				{
					var tmpr = await CosmosDbService.RestoreInstance.GetReportByIdAsync(r.Id);
					var tmpi = await CosmosDbService.Instance.GetReportByIdAsync(r.Id);

					if (tmpr.ModuleAssignment != null && tmpr.Conversation != null && tmpr.Conversation.Count >= 6)
					{
						if (tmpi == null)
						{
							await CosmosDbService.Instance.CreateReportAsync(tmpr);
						}
						else
						{
							await CosmosDbService.Instance.UpdateReportAsync(r.Id, tmpr);
						}

						// regenerate report
						await PractissWorkflow.InteractionWorkflow.RegenerateReport(r.Id);
						Console.WriteLine("Processed report " + r.Id);
					}
				}
				catch (Exception ex)
				{
					corrupted++;
				}
			}

			Console.WriteLine(corrupted);
			Console.WriteLine(alright);
		}


		static async Task RemoveOutdatedInsights()
		{
			var users = await CosmosDbService.Instance.GetAllUsersAsync();
			foreach (var user in users)
			{
				var insights = await CosmosDbService.Instance.GetInsightsAsync(user.Id);
				foreach (var insight in insights)
				{
					if (insight.QuantitativeFeedback == null)
					{
						await CosmosDbService.Instance.DeleteInsightAsync(insight);
					}
				}
			}
		}

		static async Task RegenerateTestReport()
		{
			await PractissWorkflow.InteractionWorkflow.RegenerateReport("086e0f63-8638-40a2-82d9-2f0bd66a9823");
            // await PractissWorkflow.InteractionWorkflow.RegenerateReport("6770bd0d-5ebe-4f12-b39a-ebf7b9ac12fa");
        }

        static async Task RegenerateAllReports()
		{
			var reports = await CosmosDbService.Instance.SearchReportsByLearnerOrCoachIdAsync(null, null);

            var semaphore = new SemaphoreSlim(2); // Limit to 2 concurrent tasks

            await Parallel.ForEachAsync(reports, async (report, cancellationToken) =>
            {
                await semaphore.WaitAsync(cancellationToken);

                try
                {
                    var r = await CosmosDbService.Instance.GetReportByIdAsync(report.Id);
                    if (r.Conversation.Count > 6)
                    {
                        try
                        {
                            await PractissWorkflow.InteractionWorkflow.RegenerateReport(report.Id);
                            Console.WriteLine("Processed - " + report.Id);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });
        }

		static async Task TestParsingAdditionalQuestionsFromReport()
		{
			var report = await CosmosDbService.Instance.GetReportByIdAsync("bb344e82-5250-46bf-99fa-fdd346bbba88");
			PractissWorkflow.Helpers.ExtractAdditionalQuestionsFeedback(report.MarkdownV2);
        }

		static async Task AddBetaWhitelistWave()
		{
			var lines = StringResource.Whitelisters.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var line in lines)
			{
				// Split each line by tab character
				var parts = line.Split('\t');
				var email = parts[0];
				var fullName = parts[1].Split(' ');
				var firstName = fullName[0];
				var lastName = fullName.Length > 1 ? fullName[1] : "";

				var user = await CosmosDbService.Instance.GetUserByEmail(email);
				if (user == null)
				{
					Console.WriteLine($"First Name: {firstName}, Last Name: {lastName}, Email: {email}");

					user = new CommonTypes.User
					{
						Id = Guid.NewGuid().ToString(),
						FirstName = firstName.Trim(),
						LastName = lastName.Trim(),
						Email = email.Trim().ToLower(),
						Roles = "learner"
					};

					await CosmosDbService.Instance.CreateUserAsync(user);
				}
				else
				{
					user.FirstName = firstName.Trim();
					user.LastName = lastName.Trim();
					user.Email = email.Trim().ToLower();
					await CosmosDbService.Instance.UpdateUserAsync(user.Id, user);
				}
			}
		}

        static async Task ExtractAmeliasReports()
        {
			var user = await CosmosDbService.Instance.GetUserByEmail("amelia.rosenberg@gmail.com");
            var reports = await CosmosDbService.Instance.SearchReportsByLearnerOrCoachIdAsync(user.Id, null);

            foreach (var report in reports)
            {
                try
                {
					var tmpreport = await CosmosDbService.Instance.GetReportByIdAsync(report.Id);

					File.AppendAllText("Amelia.txt", "\n\nFollowing is the report on datetime " + tmpreport.InteractionStats.InteractionStartTime);
					File.AppendAllText("Amelia.txt", "\n\n");
                    File.AppendAllText("Amelia.txt", tmpreport.MarkdownV2);
                    File.AppendAllText("Amelia.txt", "\n\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        static async Task TestGenerateV3FeedbackReports()
        {
            var user = await CosmosDbService.Instance.GetUserByEmail("joe@practiss.ai");
            var reports = await CosmosDbService.Instance.SearchReportsByLearnerOrCoachIdAsync(user.Id, null);

            foreach (var report in reports)
            {
                try
                {
                    await PractissWorkflow.InteractionWorkflow.RegenerateReport(report.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

		static async Task CleanupAvatarPersonality()
		{
			var users = await CosmosDbService.Instance.GetAllUsersAsync();

			foreach (var user in users)
			{
				//var avatars = await CosmosDbService.Instance.GetAvatarsByAuthorIdAsync(user.Id);

				//foreach (var avatar in avatars)
				//{
				//	if (avatar.Personality != null && avatar.Personality.ToLowerInvariant() != "angry")
				//	{
				//		avatar.Personality = String.Empty;
				//		await CosmosDbService.Instance.UpdateAvatarsAsync(avatar.Id, avatar);
				//	}
				//}


				var modules = await CosmosDbService.Instance.GetModulesByAuthor(user.Id);

				foreach (var module in modules)
				{
					if (module.Avatar.Personality != null && module.Avatar.Personality.ToLowerInvariant() != "angry")
					{
						if (module.Avatar.Personality != null)
						{
							module.Avatar.Personality = String.Empty;
							await CosmosDbService.Instance.UpdateModuleAsync(module.Id, module);
						}
					}
				}
			}
		}

		static async Task FixDateTimeInReports()
		{
			var reports = await CosmosDbService.Instance.SearchReportsByLearnerOrCoachIdAsync(null, null);

			foreach (var report in reports)
			{
				try
				{
					var tmpreport = await CosmosDbService.Instance.GetReportByIdAsync(report.Id);
					var startDateTime = DateTime.Parse(tmpreport.InteractionStats.InteractionStartTime);
					var endDateTime = DateTime.Parse(tmpreport.InteractionStats.InteractionEndTime);
					tmpreport.InteractionStats.InteractionStartTime = startDateTime.ToString("o");
					tmpreport.InteractionStats.InteractionEndTime = endDateTime.ToString("o");
					await CosmosDbService.Instance.UpdateReportAsync(tmpreport.Id, tmpreport);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
		}

        static async Task FixDateTimeInInsights()
        {
			var users = await CosmosDbService.Instance.GetAllUsersAsync();

			foreach (var user in users)
			{
				var insights = await CosmosDbService.Instance.GetInsightsAsync(user.Id);

				foreach (var insight in insights)
				{
					try
					{
						insight.Date = DateTime.Parse(insight.Date).ToString("o");
						await CosmosDbService.Instance.UpdateInsightAsync(insight.Id, insight);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
					}
				}
			}
        }

		static async Task GetJoesModules()
		{
			var user = await CosmosDbService.Instance.GetUserByEmail("joe@practiss.ai");
			var modules = await CosmosDbService.Instance.GetModulesByAuthor(user.Id);

			foreach(var module in modules)
			{
				Console.WriteLine("--------------------------------------------");
				Console.WriteLine(string.Format("**Title:** {0}\n\n", module.Title));
                Console.WriteLine(string.Format("**Description:** {0}\n\n", module.Description));
                Console.WriteLine(string.Format("**Situaton:** {0}\n\n", module.Situation));
                Console.WriteLine(string.Format("**Evaluation:** {0}\n\n", module.Evaluation));
                Console.WriteLine(string.Format("**Avatar:** {0}\n\n", module.Avatar.Name));
            }
            Console.WriteLine("--------------------------------------------");
        }

		static async Task TestParsingAdditionalQuestions()
		{
			foreach(var question in StringResource.AdditionQuestionsInReport.Split('\n'))
			{
				var feedback = PractissWorkflow.Helpers.ExtractAdditionalQuestionsFeedback(question);
				Console.WriteLine(feedback.Count);
			}
		}

		static async Task MakeBetaUsersDesigners()
		{
			PractissApiClientLibrary.ApiUrl = "https://practissapi.azurewebsites.net/api/";

            foreach (var user in await CosmosDbService.Instance.GetAllUsersAsync())
			{
				//if (user.Roles == null || !user.Roles.ToLowerInvariant().Contains("designer"))
				//{
				//	user.Roles += " designer";
				//	await CosmosDbService.Instance.UpdateUserAsync(user.Id, user);
				//}

				//var avatars = CosmosDbService.Instance.GetAvatarsByAuthorIdAsync(user.Id).Result.ToList();
				//PractissWeb.Utilities.Helpers.CreateDefaultAvatars(user.Id, avatars);

				Console.WriteLine(user.FirstName + " " + user.LastName);
			}
		}

		static async Task RegenerateAllReportsForClaude()
		{
			var reports = await CosmosDbService.Instance.SearchReportsByLearnerOrCoachIdAsync(null, null);

			foreach (var r in reports)
			{
				var report = await CosmosDbService.Instance.GetReportByIdAsync(r.Id);
				if (report.Conversation.Count < 6)
					continue;

				var moduleAssignment = report.ModuleAssignment;
				moduleAssignment.Module = await CosmosDbService.Instance.GetModuleAsync(moduleAssignment.Module.Id);

				var llm = new ClaudeApiClientLibrary();

				List<Message> messages = new List<Message>();

				var promptFormatString = PractissWorkflow.StringResources.ContentPromptV3;
				promptFormatString += InteractionWorkflow.GetFormattedAdditionalQuestions(moduleAssignment.Module.Evaluation);

				// Start a new message list for evaluation
				messages = new List<Message>() { new Message()
					{
						role = "system",
						content = String.Format(promptFormatString,
						moduleAssignment.Learner.FirstName,
						moduleAssignment.Module.Title,
						DateTime.Parse(report.Date).ToLongDateString(),
						moduleAssignment.Module.Description)
					}};

				messages.Add(new Message()
				{
					role = "user",
					content = InteractionWorkflow.GetTranscript(report.Conversation)
				});

				string evaluation = await llm.MakeApiRequest(messages, "large");
			}
		}

    }
}
