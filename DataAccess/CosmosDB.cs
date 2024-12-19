using CommonTypes;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
	public class _Cache
	{
		public static Dictionary<string, Avatar> avatarCache = new Dictionary<string, Avatar>();
		public static Dictionary<string, CommonTypes.User> userCache = new Dictionary<string, CommonTypes.User>();
		public static Dictionary<string, CommonTypes.Module> moduleCache = new Dictionary<string, CommonTypes.Module>();
		public static Dictionary<string, ModuleAssignment> moduleAssignmentsCache = new Dictionary<string, ModuleAssignment>();

		public static Avatar GetAvatar(string id)
		{
			lock (avatarCache)
			{
				if (avatarCache.ContainsKey(id))
					return avatarCache[id];
				else
				{
					var avatar = CosmosDbService.Instance.GetAvatarAsync(id).Result;
					avatarCache.Add(id, avatar);
					return avatar;
				}
			}
		}

		public static void UpdateAvatar(string id, Avatar avatar)
		{
			lock (avatarCache)
			{
				if (avatarCache.ContainsKey(id))
					avatarCache[id] = avatar;
				else
					avatarCache.Add(id, avatar);
			}
		}

		public static CommonTypes.User GetUser(string id)
		{
            CommonTypes.User user;

			lock (userCache)
			{
				if (userCache.ContainsKey(id))
				{
					user = userCache[id];
				}
				else
				{
					user = CosmosDbService.Instance.GetUserAsync(id).Result;

					userCache.Add(id, user);
				}
			}

			if (user.ProfileImage == null)
				user.ProfileImage = StringResources.BlankProfileBase64;

			return user;
		}

		public static void UpdateUser(string id, CommonTypes.User user)
		{
			lock (userCache)
			{
				if (userCache.ContainsKey(id))
					userCache[id] = user;
				else
					userCache.Add(id, user);
			}
		}

		public static Module GetModule(string id)
		{
			Module module;

			lock (moduleCache)
			{
				if (moduleCache.ContainsKey(id))
				{
					module = moduleCache[id];
				}
				else
				{
					module = CosmosDbService.Instance.GetModuleAsync(id).Result;
					moduleCache.Add(id, module);
				}
			}

			module.Author = GetUser(module.Author.Id);
			module.Avatar = GetAvatar(module.Avatar.Id);

			return module;
		}

		public static void UpdateModule(string id, Module module)
		{
			lock (moduleCache)
			{
				if (moduleCache.ContainsKey(id))
					moduleCache[id] = module;
				else
					moduleCache.Add(id, module);
			}
		}
	}

	public class CosmosDbService
	{
		private static string _connectionString;
		private static string _restoreConnectionString;
		private static string _databaseName;
		private static string _restoreDatabaseName;
		private Database _database;
		private static CosmosDbService _instance;
        private static CosmosDbService _restoreInstance;
		private static bool _refreshConnectionDynamically = true;

		private CosmosDbService(Database database)
		{
			try
			{
				_database = database;
            }
            catch (Exception ex)
			{
				Logger.LogError(ex.Message);
			}
		}

		public static string ConnectionString
		{
			get
			{
				if (_connectionString == null)
					_connectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString");

				return _connectionString;
			}
			set
			{
				_connectionString = value;
			}
		}

		public static string RestoreConnectionString
		{
			get
			{
				if (_restoreConnectionString == null)
					_restoreConnectionString = Environment.GetEnvironmentVariable("RestoreCosmosDbConnectionString");

				return _restoreConnectionString;
			}
			set
			{
				_restoreConnectionString = value;
			}
		}

		public static string DatabaseName
		{
			get
			{
				if (_databaseName == null)
					_databaseName = Environment.GetEnvironmentVariable("DatabaseName");

				return _databaseName;
			}
			set
			{
				_databaseName = value;
			}
		}

		public static string RestoreDatabaseName
		{
			get
			{
				if (_restoreDatabaseName == null)
					_restoreDatabaseName = Environment.GetEnvironmentVariable("RestoreDatabaseName");

				return _restoreDatabaseName;
			}
			set
			{
				_restoreDatabaseName = value;
			}
		}

		public static bool RefreshConnectionDynamically
		{
			get
			{
				return _refreshConnectionDynamically;
			}
			set
			{
				_refreshConnectionDynamically = value;
			}
		}


		public static CosmosDbService Instance
		{
			get
			{
                if (_instance != null && _refreshConnectionDynamically == false)
                {
					return _instance;
                }

                if (_instance == null || _connectionString != Environment.GetEnvironmentVariable("CosmosDbConnectionString"))
				{
					var client = new CosmosClient(ConnectionString);
					var database = client.CreateDatabaseIfNotExistsAsync(id: DatabaseName).Result;
					_instance = new CosmosDbService(database);
				}

				return _instance;
			}
		}

		public static CosmosDbService RestoreInstance
		{
			get
			{
				if (_restoreInstance != null && _refreshConnectionDynamically == false)
				{
					return _restoreInstance;
				}

				if (_restoreInstance == null || _restoreConnectionString != Environment.GetEnvironmentVariable("CosmosDbConnectionString"))
				{
					var client = new CosmosClient(ConnectionString);
					var database = client.CreateDatabaseIfNotExistsAsync(id: DatabaseName).Result;
					_restoreInstance = new CosmosDbService(database);
				}

				return _restoreInstance;
			}
		}

		public static void Init()
		{
			Instance._database.CreateContainerIfNotExistsAsync(
										id: "CoachClientRelationships",
										partitionKeyPath: "/coachid",
										throughput: 400
									).Wait();

			Instance._database.CreateContainerIfNotExistsAsync(
										id: "Users",
										partitionKeyPath: "/id",
										throughput: 400
									).Wait();

			Instance._database.CreateContainerIfNotExistsAsync(
										id: "Reports",
										partitionKeyPath: "/id",
										throughput: 400
									).Wait();

			Instance._database.CreateContainerIfNotExistsAsync(
										id: "Modules",
										partitionKeyPath: "/id",
										throughput: 400
									).Wait();

			Instance._database.CreateContainerIfNotExistsAsync(
										id: "Avatars",
										partitionKeyPath: "/id",
										throughput: 400
									).Wait();

			Instance._database.CreateContainerIfNotExistsAsync(
										id: "BookmarkedModules",
										partitionKeyPath: "/id",
										throughput: 400
									).Wait();

			Instance._database.CreateContainerIfNotExistsAsync(
										id: "ModuleAssignments",
										partitionKeyPath: "/id",
										throughput: 400
									).Wait();

			Instance._database.CreateContainerIfNotExistsAsync(
										id: "Insights",
										partitionKeyPath: "/userId",
										throughput: 400
									).Wait();
		}

		#region User CRUD

		public async Task<CommonTypes.User> CreateUserAsync(CommonTypes.User user)
		{
			user.Email = user.Email.ToLowerInvariant();

			var container = _database.GetContainer("Users");
			var result = await container.CreateItemAsync(user, new PartitionKey(user.Id));
			return result.Resource;
		}

		public async Task<CommonTypes.User> GetUserAsync(string id)
		{
			var container = _database.GetContainer("Users");
			try
			{
				ItemResponse<CommonTypes.User> response = await container.ReadItemAsync<CommonTypes.User>(id, new PartitionKey(id));
				var user = response.Resource;

				user.Email = user.Email.ToLowerInvariant();

				if (user.ProfileImage == null)
					user.ProfileImage = StringResources.BlankProfileBase64;

				return user;
			}
			catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return null;
			}
		}

        public async Task<CommonTypes.User> UpdateUserAsync(string id, CommonTypes.User user)
		{
			var container = _database.GetContainer("Users");
			var result = await container.ReplaceItemAsync(user, id, new PartitionKey(id));
			return result.Resource;
		}

		public async Task DeleteUserAsync(string id)
		{
			var container = _database.GetContainer("Users");
			await container.DeleteItemAsync<CommonTypes.User>(id, new PartitionKey(id));
		}

		public async Task<CommonTypes.User> GetUserByEmail(string email)
		{
			try
			{
				email = email.ToLowerInvariant();

				var container = _database.GetContainer("Users");
				var query = new QueryDefinition("SELECT * FROM c WHERE c.Email = @email")
								.WithParameter("@email", email);

				var iterator = container.GetItemQueryIterator<CommonTypes.User>(query);

				while (iterator.HasMoreResults)
				{
					var response = await iterator.ReadNextAsync();
					var user = response.FirstOrDefault();
					if (user != null)
					{
						if (user.AdminPrompts == null)
							user.AdminPrompts = new Admin();

						user.AdminPrompts.AdditionalInstructionsForResponsePrompt = user.AdminPrompts.AdditionalInstructionsForResponsePrompt;
						user.AdminPrompts.AdditionalInstructionsForEvaluationPrompt = user.AdminPrompts.AdditionalInstructionsForEvaluationPrompt;
						user.AdminPrompts.CustomField = user.AdminPrompts.CustomField;

						return user;
					}
				}
			}catch (Exception ex)
			{
				Logger.LogError(ex.Message);
			}

			return null; // User not found
		}

		public async Task<List<CommonTypes.User>> GetAllUsersAsync()
		{
			var container = _database.GetContainer("Users");
			var query = new QueryDefinition("SELECT * FROM c");

			var iterator = container.GetItemQueryIterator<CommonTypes.User>(query);
			var results = new List<CommonTypes.User>();

			while (iterator.HasMoreResults)
			{
				var response = await iterator.ReadNextAsync();
				results.AddRange(response.ToList());
			}

            foreach (var item in results)
                item.ProfileImage = null;

            return results.ToList();
		}

        public async Task<List<CommonTypes.User>> GetAllUsersByDomainAsync(string domain)
        {
            var container = _database.GetContainer("Users");
            var query = new QueryDefinition("SELECT * FROM c WHERE  CONTAINS(LOWER(c.Email), @domain)")
				.WithParameter("@domain", domain.ToLower());

            var iterator = container.GetItemQueryIterator<CommonTypes.User>(query);
            var results = new List<CommonTypes.User>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

			foreach (var item in results)
				item.ProfileImage = null;

            return results.ToList();
        }

        #endregion

        #region Modules CRUD

        public async Task<Module> CreateModuleAsync(Module module)
		{
			try
			{
				var container = _database.GetContainer("Modules");
				var result = await container.CreateItemAsync(module, new PartitionKey(module.Id));
				return result.Resource;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<Module> GetModuleAsync(string id)
		{
			try
			{
				var container = _database.GetContainer("Modules");
				try
				{
					ItemResponse<Module> response = await container.ReadItemAsync<Module>(id, new PartitionKey(id));
					return response.Resource;
				}
				catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					return null;
				}
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<Module> UpdateModuleAsync(string id, Module module)
		{
			try
			{
				var container = _database.GetContainer("Modules");
				var result = await container.ReplaceItemAsync(module, id, new PartitionKey(id));
				return result.Resource;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task DeleteModuleAsync(string id)
		{
			try
			{
				var container = _database.GetContainer("Modules");
				await container.DeleteItemAsync<Module>(id, new PartitionKey(id));
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<IEnumerable<Module>> GetModulesByAuthor(string authorId)
		{
			try
			{
				var container = _database.GetContainer("Modules");
				var query = new QueryDefinition("SELECT * FROM Modules m WHERE m.Author.id = @authorId ORDER BY m._ts DESC")
								.WithParameter("@authorId", authorId);

				var iterator = container.GetItemQueryIterator<Module>(query);
				var results = new List<Module>();

				while (iterator.HasMoreResults)
				{
					var response = await iterator.ReadNextAsync();
					results.AddRange(response.ToList());
				}

				return results;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<IEnumerable<Module>> SearchForModule(string moduleName, string authorName)
		{
			try
			{
				if(moduleName == "empty")
					moduleName = null;

				if(authorName == "empty")
					authorName = null;

				var container = _database.GetContainer("Modules");

				// Building the query with CONTAINS to search by moduleName or authorName
				string queryString = "SELECT * FROM Modules m WHERE m.Visibility = \"Public\"";

				if (moduleName != null)
					queryString += " AND CONTAINS(LOWER(m.Title), @moduleName)";

				if (authorName != null)
					queryString += " AND CONTAINS(LOWER(CONCAT(CONCAT(m.Author.FirstName, \" \"), m.Author.LastName)), @authorName)";

				queryString += " ORDER BY m._ts DESC";


				QueryDefinition query = null;

				if (moduleName != null && authorName == null)
					query = new QueryDefinition(queryString)
								.WithParameter("@moduleName", moduleName.ToLower());

				if (moduleName == null && authorName != null)
					query = new QueryDefinition(queryString)
								.WithParameter("@authorName", authorName.ToLower());

				if (moduleName != null && authorName != null)
					query = new QueryDefinition(queryString)
								.WithParameter("@moduleName", moduleName.ToLower())
								.WithParameter("@authorName", authorName.ToLower());


				var iterator = container.GetItemQueryIterator<Module>(query);
				var results = new List<Module>();

				while (iterator.HasMoreResults)
				{
					var response = await iterator.ReadNextAsync();
					results.AddRange(response.ToList());
				}

				// TODO - figure out why cosmos db is returning private modules
				return results.Where(m => m.Visibility.ToLower() == "public");
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		#endregion


		#region Avatars CRUD

		public async Task<Avatar> CreateAvatarAsync(Avatar avatar)
		{
			try
			{
				var container = _database.GetContainer("Avatars");
				var result = await container.CreateItemAsync(avatar, new PartitionKey(avatar.Id));
				return result.Resource;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<Avatar> GetAvatarAsync(string id)
		{
			try
			{
				var container = _database.GetContainer("Avatars");
				try
				{
					ItemResponse<Avatar> response = await container.ReadItemAsync<Avatar>(id, new PartitionKey(id));
					return response.Resource;
				}
				catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					return null;
				}
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<Avatar> UpdateAvatarsAsync(string id, Avatar avatar)
		{
			try
			{
				var container = _database.GetContainer("Avatars");
				var result = await container.ReplaceItemAsync(avatar, id, new PartitionKey(id));
				return result.Resource;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task DeleteAvatarAsync(string id)
		{
			try
			{
				var container = _database.GetContainer("Avatars");
				await container.DeleteItemAsync<Avatar>(id, new PartitionKey(id));
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<IEnumerable<Avatar>> GetAvatarsByAuthorIdAsync(string authorId)
		{
			try
			{
				var container = _database.GetContainer("Avatars");
				var query = new QueryDefinition("SELECT * FROM Avatars a WHERE a.AuthorId = @authorId ORDER BY a._ts DESC")
								.WithParameter("@authorId", authorId);

				var iterator = container.GetItemQueryIterator<Avatar>(query);
				var results = new List<Avatar>();

				while (iterator.HasMoreResults)
				{
					var response = await iterator.ReadNextAsync();
					results.AddRange(response.ToList());
				}

				return results;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		#endregion

		#region ModuleAssignments CRUD

		public async Task<ModuleAssignment> CreateModuleAssignmentAsync(ModuleAssignment moduleAssignment)
		{
			try
			{
				moduleAssignment = moduleAssignment.Sanitize();

				var container = _database.GetContainer("ModuleAssignments");
				return await container.CreateItemAsync(moduleAssignment, new PartitionKey(moduleAssignment.Id));
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<ModuleAssignment> UpdateModuleAssignmentAsync(string id, ModuleAssignment moduleAssignment)
		{
			try
			{
				moduleAssignment = moduleAssignment.Sanitize();

				var container = _database.GetContainer("ModuleAssignments");
				return await container.ReplaceItemAsync(moduleAssignment, id, new PartitionKey(moduleAssignment.Id));
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task DeleteModuleAssignmentAsync(string id)
		{
			try
			{
				var container = _database.GetContainer("ModuleAssignments");
				await container.DeleteItemAsync<ModuleAssignment>(id, new PartitionKey(id));
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		private async Task RefreshObject(List<ModuleAssignment> moduleAssigments)
		{
			foreach (var item in moduleAssigments)
			{
				item.Learner = await GetUserAsync(item.Learner.Id);
				item.Coach = await GetUserAsync(item.Coach.Id);
				item.Module = await GetModuleAsync(item.Module.Id);
			}
		}

		public async Task<IEnumerable<ModuleAssignment>> GetModuleAssignmentByLearnerAsync(string learnerId)
		{
			try
			{
				var container = _database.GetContainer("ModuleAssignments");
				var query = new QueryDefinition(
							"SELECT * FROM ModuleAssignments m WHERE m.Learner.id = @learnerId AND (NOT IS_DEFINED(m.Hidden) OR m.Hidden = false) ORDER BY m._ts DESC")
							.WithParameter("@learnerId", learnerId);

				var iterator = container.GetItemQueryIterator<ModuleAssignment>(query);
				var results = new List<ModuleAssignment>();

				while (iterator.HasMoreResults)
				{
					var response = await iterator.ReadNextAsync();
					results.AddRange(response.ToList());
				}

				await RefreshObject(results);

				return results;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<IEnumerable<ModuleAssignment>> SearchModuleAssignmentByCoachAsync(string coachId, string moduleId)
		{
			try
			{
				var container = _database.GetContainer("ModuleAssignments");
				var query = new QueryDefinition(
							"SELECT * FROM ModuleAssignments m WHERE m.Module.id = @moduleId AND m.Coach.id = @coachId AND (NOT IS_DEFINED(m.Hidden) OR m.Hidden = false) ORDER BY m._ts DESC")
							.WithParameter("@moduleId", moduleId)
							.WithParameter("@coachId", coachId);


				var iterator = container.GetItemQueryIterator<ModuleAssignment>(query);
				var results = new List<ModuleAssignment>();

				while (iterator.HasMoreResults)
				{
					var response = await iterator.ReadNextAsync();
					results.AddRange(response.ToList());
				}

				await RefreshObject(results);

				return results;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<ModuleAssignment>GetModuleAssignmentByIdAsync(string moduleAssignmentId)
		{
			try
			{
				var container = _database.GetContainer("ModuleAssignments");
				var query = new QueryDefinition(
					"SELECT * FROM ModuleAssignments m WHERE m.id = @id")
					.WithParameter("@id", moduleAssignmentId);

				var iterator = container.GetItemQueryIterator<ModuleAssignment>(query);
				var results = new List<ModuleAssignment>();

				while (iterator.HasMoreResults)
				{
					var response = await iterator.ReadNextAsync();
					results.AddRange(response.ToList());
				}

				await RefreshObject(results);

				var moduleAssignment = results.FirstOrDefault();

				return moduleAssignment;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<ModuleAssignment> GetModuleAssignmentByCoachModuleLearnerAsync(string coachId, string moduleId, string learnerId)
		{
			try
			{
				var container = _database.GetContainer("ModuleAssignments");
				var query = new QueryDefinition(
					"SELECT * FROM ModuleAssignments m WHERE m.Module.id = @moduleId AND m.Coach.id = @coachId AND m.Learner.id = @learnerId")
					.WithParameter("@moduleId", moduleId)
					.WithParameter("@learnerId", learnerId)
					.WithParameter("@coachId", coachId);

				var iterator = container.GetItemQueryIterator<ModuleAssignment>(query);
				var results = new List<ModuleAssignment>();

				while (iterator.HasMoreResults)
				{
					var response = await iterator.ReadNextAsync();
					results.AddRange(response.ToList());
				}

				await RefreshObject(results);

				var moduleAssignment = results.FirstOrDefault();

				return moduleAssignment;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}


		#endregion

		#region BookmarkedModules CRUD

		public async Task<BookmarkedModule> CreateBookmarkedModuleAsync(BookmarkedModule bookmarkedModule)
		{
			try
			{
				bookmarkedModule = bookmarkedModule.Sanitize();

				var container = _database.GetContainer("BookmarkedModules");
				return await container.CreateItemAsync(bookmarkedModule, new PartitionKey(bookmarkedModule.Id));
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task RemoveBookmarkedModuleAsync(string id)
		{
			try
			{
				var container = _database.GetContainer("BookmarkedModules");
				await container.DeleteItemAsync<BookmarkedModule>(id, new PartitionKey(id));
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task RefreshObject(List<BookmarkedModule> bookmarkedModules)
		{
			foreach (var item in bookmarkedModules)
			{
				item.User = await GetUserAsync(item.User.Id);
				item.Module = await GetModuleAsync(item.Module.Id);
			}
		}

		public async Task<IEnumerable<BookmarkedModule>> GetBookmarkedModulesByUserIdAsync(string userId)
		{
			try
			{
				var container = _database.GetContainer("BookmarkedModules");
				var query = new QueryDefinition("SELECT * FROM BookmarkedModules b WHERE b.User.id = @userId ORDER BY b._ts DESC")
								.WithParameter("@userId", userId);

				var iterator = container.GetItemQueryIterator<BookmarkedModule>(query);
				var results = new List<BookmarkedModule>();

				while (iterator.HasMoreResults)
				{
					var response = await iterator.ReadNextAsync();
					results.AddRange(response.ToList());
				}

				await RefreshObject(results);

				return results;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}


		#endregion

		#region Reports CRUD

		public async Task CreateReportAsync(Report report)
		{
			try
			{
				report = report.Sanitize();
				var container = _database.GetContainer("Reports");
				await container.CreateItemAsync(report, new PartitionKey(report.Id));
				return;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<IEnumerable<Report>> SearchReportsByLearnerOrCoachIdAsync(string learnerId, string coachId)
		{
			try
			{
				var container = _database.GetContainer("Reports");

				QueryDefinition query = null;

				var columnsNeeded = "r.id, r.Date, r.InteractionStats, r.ModuleAssignment.id AS ModuleAssignmentId, r.ModuleAssignment.Module.id AS ModuleId, r.ModuleAssignment.Module.Title AS ModuleTitle, r.ModuleAssignment.Coach.id AS CoachId, r.ModuleAssignment.Coach.FirstName AS CoachFirstName, r.ModuleAssignment.Coach.LastName AS CoachLastName, r.ModuleAssignment.Coach.Email AS CoachEmail, r.ModuleAssignment.Learner.id AS LearnerId, r.ModuleAssignment.Learner.FirstName AS LearnerFirstName, r.ModuleAssignment.Learner.LastName AS LearnerLastName, r.ModuleAssignment.Learner.Email AS LearnerEmail";
				
                if (learnerId == null & coachId == null)
				{
					// AllReports mode
					query = new QueryDefinition(
                    "SELECT * FROM Reports r ORDER BY r.InteractionStats.InteractionStartTime DESC".Replace("*", columnsNeeded));
				}
				else if (learnerId == null && coachId != null)
				{
					query = new QueryDefinition(
                    "SELECT * FROM Reports r WHERE r.ModuleAssignment.Coach.id = @coachId AND r.Submitted = true ORDER BY r.InteractionStats.InteractionStartTime DESC".Replace("*", columnsNeeded))
					.WithParameter("@coachId", coachId);
				}
				else if (learnerId != null && coachId == null)
				{
					query = new QueryDefinition(
                    "SELECT * FROM Reports r WHERE r.ModuleAssignment.Learner.id = @learnerId ORDER BY r.InteractionStats.InteractionStartTime DESC".Replace("*", columnsNeeded))
					.WithParameter("@learnerId", learnerId);
				}
				else if (learnerId != null && coachId != null)
				{
					query = new QueryDefinition(
                    "SELECT * FROM Reports r WHERE r.ModuleAssignment.Learner.id = @learnerId AND r.ModuleAssignment.Coach.id = @coachId AND r.Submitted = true ORDER BY r.InteractionStats.InteractionStartTime DESC".Replace("*", columnsNeeded))
					.WithParameter("@learnerId", learnerId)
					.WithParameter("@coachId", coachId);
				}

				var iterator = container.GetItemQueryIterator<ReportDTO>(query); // Use ReportDTO
				var results = new List<Report>();

				while (iterator.HasMoreResults)
				{
					var response = await iterator.ReadNextAsync();
					var dtos = response.ToList();

					// Map from ReportDTO to Report
					foreach (var dto in dtos)
					{
						var report = MapToReport(dto); // Implement this method to map from DTO to Report
						results.Add(report);
					}
				}

				foreach (var report in results)
				{
					report.RoleplayMinutes = (int)report.InteractionStats.RoleplayMinutes;
				}

				return results;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<IEnumerable<Report>> SearchAbortedReportsByLearnerOrCoachIdAsync(string learnerId, string coachId)
		{
			try
			{
				var container = _database.GetContainer("Reports");

				QueryDefinition query = null;

				var columnsNeeded = "r.id, r.Date, r.InteractionStats, r.ModuleAssignment.id AS ModuleAssignmentId, r.ModuleAssignment.Module.id AS ModuleId, r.ModuleAssignment.Module.Title AS ModuleTitle, r.ModuleAssignment.Coach.id AS CoachId, r.ModuleAssignment.Coach.FirstName AS CoachFirstName, r.ModuleAssignment.Coach.LastName AS CoachLastName, r.ModuleAssignment.Coach.Email AS CoachEmail, r.ModuleAssignment.Learner.id AS LearnerId, r.ModuleAssignment.Learner.FirstName AS LearnerFirstName, r.ModuleAssignment.Learner.LastName AS LearnerLastName, r.ModuleAssignment.Learner.Email AS LearnerEmail";

				if (learnerId == null & coachId == null)
				{
					// AllReports mode
					query = new QueryDefinition(
					"SELECT * FROM Reports r WHERE ARRAY_LENGTH(r.Conversation) < 4 ORDER BY r.InteractionStats.InteractionStartTime DESC".Replace("*", columnsNeeded));
				}
				else if (learnerId == null && coachId != null)
				{
					query = new QueryDefinition(
					"SELECT * FROM Reports r WHERE r.ModuleAssignment.Coach.id = @coachId AND r.Submitted = true AND ARRAY_LENGTH(r.Conversation) < 4 ORDER BY r.InteractionStats.InteractionStartTime DESC".Replace("*", columnsNeeded))
					.WithParameter("@coachId", coachId);
				}
				else if (learnerId != null && coachId == null)
				{
					query = new QueryDefinition(
					"SELECT * FROM Reports r WHERE r.ModuleAssignment.Learner.id = @learnerId AND ARRAY_LENGTH(r.Conversation) < 4 ORDER BY r.InteractionStats.InteractionStartTime DESC".Replace("*", columnsNeeded))
					.WithParameter("@learnerId", learnerId);
				}
				else if (learnerId != null && coachId != null)
				{
					query = new QueryDefinition(
					"SELECT * FROM Reports r WHERE r.ModuleAssignment.Learner.id = @learnerId AND r.ModuleAssignment.Coach.id = @coachId AND r.Submitted = true AND ARRAY_LENGTH(r.Conversation) < 4 ORDER BY r.InteractionStats.InteractionStartTime DESC".Replace("*", columnsNeeded))
					.WithParameter("@learnerId", learnerId)
					.WithParameter("@coachId", coachId);
				}

				var iterator = container.GetItemQueryIterator<ReportDTO>(query); // Use ReportDTO
				var results = new List<Report>();

				while (iterator.HasMoreResults)
				{
					var response = await iterator.ReadNextAsync();
					var dtos = response.ToList();

					// Map from ReportDTO to Report
					foreach (var dto in dtos)
					{
						var report = MapToReport(dto); // Implement this method to map from DTO to Report
						results.Add(report);
					}
				}

				foreach (var report in results)
				{
					report.RoleplayMinutes = (int)report.InteractionStats.RoleplayMinutes;
				}

				return results;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<Report> GetReportByIdAsync(string reportId)
		{
			try
			{
				var container = _database.GetContainer("Reports");
				var query = new QueryDefinition(
					"SELECT * FROM Reports r WHERE r.id = @reportId")
					.WithParameter("@reportId", reportId);

				var iterator = container.GetItemQueryIterator<Report>(query);
				var results = new List<Report>();

				while (iterator.HasMoreResults)
				{

					var response = await iterator.ReadNextAsync();
					results.AddRange(response.ToList());
				}

				var report = results.FirstOrDefault();

				return report;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<Report> UpdateReportAsync(string id, Report report)
		{
			try
			{
				report = report.Sanitize();
				var container = _database.GetContainer("Reports");
				var result = await container.ReplaceItemAsync(report, id, new PartitionKey(id));
				return result.Resource;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task DeleteReportAsync(string id)
		{
			try
			{
				var container = _database.GetContainer("Reports");
				await container.DeleteItemAsync<Report>(id, new PartitionKey(id));
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}


		#endregion

		#region CoachClientRelationship CRUD

		public async Task<CoachClientRelationship> CreateClientAsync(CommonTypes.CoachClientRelationship client)
		{
			try
			{
				var container = _database.GetContainer("CoachClientRelationships");
				var result = await container.CreateItemAsync(client, new PartitionKey(client.CoachId));
				return result.Resource;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<IEnumerable<CoachClientRelationship>> GetClientsAsync(string coachId)
		{
			try
			{
				var container = _database.GetContainer("CoachClientRelationships");
				var query = new QueryDefinition(
					"SELECT * FROM CoachClientRelationships c WHERE c.coachid = @coachId ORDER BY c._ts DESC")
					.WithParameter("@coachId", coachId);

				var iterator = container.GetItemQueryIterator<CoachClientRelationship>(query);
				var results = new List<CoachClientRelationship>();

				while (iterator.HasMoreResults)
				{

					var response = await iterator.ReadNextAsync();
					results.AddRange(response.ToList());
				}

				return results;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task DeleteClientAsync(string relationshipId, string coachId)
		{
			var container = _database.GetContainer("CoachClientRelationships");
			await container.DeleteItemAsync<CommonTypes.CoachClientRelationship>(relationshipId, new PartitionKey(coachId));
		}

		#endregion


		#region Insights

		public async Task<Insight> CreateInsightAsync(Insight insight)
		{
			try
			{
				var container = _database.GetContainer("Insights");
				var result = await container.CreateItemAsync(insight, new PartitionKey(insight.UserId));
				return result.Resource;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<List<Insight>> GetInsightsAsync(string userId)
		{
			try
			{
				try
				{
					var container = _database.GetContainer("Insights");
					var query = new QueryDefinition(
						"SELECT * FROM Insights r WHERE r.userId = @userId ORDER BY r.Date DESC")
						.WithParameter("@userId", userId);

					var iterator = container.GetItemQueryIterator<Insight>(query);
					var results = new List<Insight>();

					while (iterator.HasMoreResults)
					{

						var response = await iterator.ReadNextAsync();
						results.AddRange(response);
					}

					return results;
				}
				catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					return null;
				}
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<Insight> GetInsightByIdAsync(string userId, string insightId)
		{
			try
			{
				try
				{
					var container = _database.GetContainer("Insights");
					var query = new QueryDefinition(
						"SELECT * FROM Insights r WHERE r.userId = @userId AND r.id = @insightId ORDER BY r._ts DESC")
						.WithParameter("@userId", userId)
						.WithParameter("@insightId", insightId);

					var iterator = container.GetItemQueryIterator<Insight>(query);
					var results = new List<Insight>();

					while (iterator.HasMoreResults)
					{

						var response = await iterator.ReadNextAsync();
						results.AddRange(response);
					}

					return results.FirstOrDefault();
				}
				catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					return null;
				}
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<Insight> GetInsightByTitleAsync(string userId, string title)
		{
			try
			{
				try
				{
					var container = _database.GetContainer("Insights");
					var query = new QueryDefinition(
						"SELECT * FROM Insights r WHERE r.userId = @userId AND r.Title = @title ORDER BY r._ts DESC")
						.WithParameter("@userId", userId)
						.WithParameter("@title", title);

					var iterator = container.GetItemQueryIterator<Insight>(query);
					var results = new List<Insight>();

					while (iterator.HasMoreResults)
					{

						var response = await iterator.ReadNextAsync();
						results.AddRange(response);
					}

					return results.FirstOrDefault();
				}
				catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					return null;
				}
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<Insight> UpdateInsightAsync(string id, Insight insight)
		{
			try
			{
				var container = _database.GetContainer("Insights");
				var result = await container.ReplaceItemAsync(insight, id, new PartitionKey(insight.UserId));
				return result.Resource;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task DeleteAllInsightsAsync()
		{
			try
			{
				var container = _database.GetContainer("Insights");
				var query = new QueryDefinition("SELECT * FROM Insights r");

				var iterator = container.GetItemQueryIterator<Insight>(query);
				var results = new List<Insight>();

				while (iterator.HasMoreResults)
				{

					var response = await iterator.ReadNextAsync();
					results.AddRange(response);
				}

				foreach(var insight in results)
				{
					await container.DeleteItemAsync<Insight>(insight.Id, new PartitionKey(insight.UserId));
				}

				return;
			}
			catch (CosmosException ex)
			{
				Logger.LogError(ex.Message);
				throw;
			}
		}

        public async Task DeleteInsightAsync(Insight insight)
        {
            try
            {
                var container = _database.GetContainer("Insights");
                await container.DeleteItemAsync<Insight>(insight.Id, new PartitionKey(insight.UserId));
            }
            catch (CosmosException ex)
            {
                Logger.LogError(ex.Message);
                throw;
            }
        }

        #endregion



        #region DTO Maps

        public static Report MapToReport(ReportDTO dto)
		{
			var report = new Report
			{
				Id = dto.Id,
				Date = dto.Date,
				InteractionStats = new InteractionStats
				{
					InteractionStartTime = dto.InteractionStats.InteractionStartTime,
					InteractionEndTime = dto.InteractionStats.InteractionEndTime,
					RoleplayMinutes = dto.InteractionStats.RoleplayMinutes
				},
				ModuleAssignment = new ModuleAssignment
				{
					Id = dto.ModuleAssignmentId,
					Module = new Module
					{
						Id = dto.ModuleId,
						Title = dto.ModuleTitle
					},
					Coach = new CommonTypes.User
					{
						Id = dto.CoachId,
						FirstName = dto.CoachFirstName,
						LastName = dto.CoachLastName,
						Email = dto.CoachEmail
					},
					Learner = new CommonTypes.User
					{
						Id = dto.LearnerId,
						FirstName = dto.LearnerFirstName,
						LastName = dto.LearnerLastName,
						Email = dto.LearnerEmail
					}
				},
				CurrentFeedback = new List<Feedback>(),
				PriorFeedbacksV2 = new List<PriorFeedback>()
			};

			return report;
		}


		#endregion
	}
}
