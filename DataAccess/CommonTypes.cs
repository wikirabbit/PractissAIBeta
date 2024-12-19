using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CommonTypes
{
    public enum LLMType
    {
        OpenAI,
        Gemini,
        Mistral
    }

	public class Attendee
	{
		public string DisplayName { get; set; }
		public string Email { get; set; }
	}

    public class Feedback
    {
        public string ExerciseId { get; set; }
        public User FeedbackProvider { get; set; }
        public List<Reaction> ReportReactions { get; set; }
		public List<Reaction> InteractionReactions { get; set; }

        public Feedback() 
        {
            ReportReactions = new List<Reaction>();
            InteractionReactions = new List<Reaction>();
        }  
	}

    public class PriorFeedback
    {
        /// <summary>
        /// Deprecated
        /// </summary>
        public string Markdown { get; set; }
        /// <summary>
        /// Deprecated
        /// </summary>
        public string Markdown2 { get; set; }
        public string Markdown3 { get; set; }
        public List<Message> Conversation { get; set; }
        public List<Feedback> Feedbacks { get; set; }

        public PriorFeedback()
        {
            Conversation = new List<Message>();
            Feedbacks = new List<Feedback>();
        }
    }

    public class Reaction
    {
        public int Index { get; set; }
        public bool ThumbsUp { get; set; }
        public bool ThumbsDown { get; set; }
        public string Comments { get; set; }
    }

	public enum ModuleVisibility
	{
		Public,
		Private
	}

	public class Message
	{
		public string role { get; set; }
		public string content { get; set; }
	}

	public class WhitelistUser
	{
		public string Email { get; set; }
		public string Name { get; set; }
		public string CompanyName { get; set; }
		public int MaximumClients { get; set; }
		public string GroupName { get; set; }
	}

	public class UserStats
    {
        public int InteractionsCount { get; set; }
        public string LastInteractionDateTime { get; set; }
        public double TotalRoleplayMinutes { get; set; }
        public int ModulesCreated { get; set; }
    }

	public class UserStatsForReporting
	{
		public CommonTypes.User User { get; set; }
		public long TimeOnPlatform { get; set; }
		public long DaysInactive { get; set; }
		public long SessionsCompleted { get; set; }
		public long SessionsAborted { get; set; }
		public long CommentsProvided { get; set; }
		public long ReactionsProvided { get; set; }

		public UserStatsForReporting() { }
	}


	public class Integrations
    {
        public string FirefliesApiKey { get; set; }
    }

    public class InteractionStats
    {
        public string InteractionStartTime { get; set; }
        public string InteractionEndTime { get; set; }
        public double RoleplayMinutes {  get; set; }

        public InteractionStats() 
        {
            // For Backcompat,  in case some values are null in the json
            InteractionStartTime = DateTime.MinValue.ToString();
            InteractionEndTime = DateTime.MinValue.ToString();
        }
            
    }

    public class Admin
    {
		public string AdditionalInstructionsForResponsePrompt { get; set; }
		public string AdditionalInstructionsForEvaluationPrompt { get; set; }
        public string CustomField { get; set; }
	}

    public class UserStatus
    {
        public bool IsInactive { get; set; }
        public string SubscriptionActiveUntil { get; set; }

        public UserStatus() 
        {
            SubscriptionActiveUntil = DateTime.MinValue.ToString("o");
        }
    }

	public class User
    {
		[JsonProperty("id")]
		public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Bio { get; set; }
        public string ProfileImage { get; set; }
        public Admin AdminPrompts { get; set; }
        public string Roles { get; set; }
        public UserStats UserStats { get; set; }
        public bool AlertOnError { get; set; }
		public bool ShowIntenseAvatars { get; set; }
		public bool StreamingMode { get; set; }
        public LLMType RoleplayLLM { get; set; }
        public LLMType ReportLLM { get; set; }
        public UserStatus UserStatus { get; set; }

        public Integrations Integrations { get; set; }
		public Dictionary<string, long> RecommendationWatermarks { get; set; }
        public string LastLoginTime { get; set; }

		public User()
        {
            RoleplayLLM = LLMType.OpenAI;
            ReportLLM = LLMType.OpenAI;

            AdminPrompts = new Admin();
            UserStats = new UserStats();
            Integrations = new Integrations();
            UserStatus = new UserStatus();
            RecommendationWatermarks = new Dictionary<string, long>();
            StreamingMode = true;
        }
    }

	public class Module
    {
		[JsonProperty("id")]
		public string Id { get; set; }
        public string Title { get; set; }
		public string OriginalUserPrompt { get; set; }
		public string Description { get; set; }

        public string Situation { get; set; }
        public string Evaluation { get; set; }

        public string Visibility { get; set; }
        public User Author { get; set; }
        public Avatar Avatar { get; set; }

        public int InteractionsCount { get; set; }

		public int? _ttl { get; set; } // TTL in seconds. Nullable int to make it optional.

	}

	public class Avatar
    {
		[JsonProperty("id")]
		public string Id { get; set; }
        public string Image { get; set; }
		public string ImageCode { get; set; }
		public string Name { get; set; }
        public string Personality { get; set; }
        public string VoiceName { get; set; }
        public string VoiceSampleUrl { get; set; }
        public string AuthorId { get; set; }
    }

    public class ModuleAssignment
    {
		[JsonProperty("id")]
		public string Id { get; set; }
        // ModuleAssignments are created in Hidden mode,
        // when user is directly trying out a module from
        // the module explorer. We don't want to show this
        // in their list of modules in their module library
        // unless they bookmark.
        // Same if they are spinning up a auto created module
        // for Contextual scenarios.
        public bool Hidden { get; set; }
        public Module Module { get; set; }
        public User Learner { get; set; }
        public User Coach { get; set; }

        public int InteractionsAllowed { get; set; }
        public int InteractionsCount { get; set; }

        public ModuleAssignment Sanitize()
        {
            var clone = new ModuleAssignment();
            clone.Id = Id;
            clone.Module = new Module() { Id = Module.Id };
            clone.Learner = new User() { Id = Learner.Id };
            clone.Coach = new User() { Id = Coach.Id };
            clone.InteractionsAllowed = InteractionsAllowed;
            clone.InteractionsCount = InteractionsCount;
            clone.Hidden = Hidden;
            return clone;
        }

		public int? _ttl { get; set; } // TTL in seconds. Nullable int to make it optional.
	}

	public class BookmarkedModule
    {
		[JsonProperty("id")]
		public string Id { get; set; }
        public Module Module { get; set; }
        public User User { get; set; }

        public BookmarkedModule Sanitize()
        {
            var clone = new BookmarkedModule();
            clone.Id = Id;
            clone.Module = new Module() { Id = Module.Id }; ;
            clone.User = new User() { Id = User.Id };
                
            return clone;
        }
    }

	public class CoachClientRelationship
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("coachid")]
		public string CoachId { get; set; }
		public User Learner { get; set; }
	}

    public class Report
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public ModuleAssignment ModuleAssignment { get; set; }
        public List<Message> Conversation { get; set; }
        /// <summary>
        /// Deprecated
        /// </summary>
        public string Markdown { get; set; }
		/// <summary>
		/// Deprecated
		/// </summary>
		public string MarkdownV2 { get; set; }
		public string MarkdownV3 { get; set; }
		/// <summary>
		/// Deprecated
		/// </summary>
		public QuantitativeFeedback QuantitativeFeedback { get; set; }
		/// <summary>
		/// Deprecated
		/// </summary>
		public QuantitativeFeedbackV2 QuantitativeFeedbackV2 { get; set; }
		public QuantitativeFeedbackV3 QuantitativeFeedbackV3 { get; set; }
		public Dictionary<string, bool> AdditionalQuestions { get; set; } 
        public string Date { get; set; }
        public string Summary { get; set; }
        public bool Submitted { get; set; }
        public string SelfReportedConfidenceLevel { get; set; }
        public InteractionStats InteractionStats { get; set; }
        public List<Feedback> CurrentFeedback { get; set; }
        /// <summary>
        /// Deprecated. Use PriorFeedbackV2 instead.
        /// </summary>
        public List<List<Feedback>> PriorFeedbacks { get; set; }
        public List<PriorFeedback> PriorFeedbacksV2 { get; set; }
        public int RoleplayMinutes { get; set; }
        public int FeedbackCommentsCount { get; set; }
        public int FeedbackReactionsCount { get; set; }
        public string Tag { get; set; }

        public Report()
        {
            InteractionStats = new InteractionStats();
            CurrentFeedback = new List<Feedback>();
            PriorFeedbacksV2 = new List<PriorFeedback>();
            AdditionalQuestions = new Dictionary<string, bool>();
        }

        public Report Sanitize()
        {
            ModuleAssignment.Coach = new User()
            {
                Id = ModuleAssignment.Coach.Id,
                FirstName = ModuleAssignment.Coach.FirstName,
                LastName = ModuleAssignment.Coach.LastName,
                Email = ModuleAssignment.Coach.Email
            };

			ModuleAssignment.Learner = new User()
			{
                Id = ModuleAssignment.Learner.Id,
				FirstName = ModuleAssignment.Learner.FirstName,
				LastName = ModuleAssignment.Learner.LastName,
				Email = ModuleAssignment.Learner.Email
			};

            ModuleAssignment.Module = new Module()
            {
                Id = ModuleAssignment.Module.Id,
                Title = ModuleAssignment.Module.Title
            };

            foreach(var feedback in CurrentFeedback)
            {
                feedback.FeedbackProvider = new User()
                {
                    Id = feedback.FeedbackProvider.Id,
                    FirstName = feedback.FeedbackProvider.FirstName,
                    LastName = feedback.FeedbackProvider.LastName,
                    Email = feedback.FeedbackProvider.Email
                };
            }

            foreach(var priorFeedback in PriorFeedbacksV2)
            {
                foreach (var feedback in priorFeedback.Feedbacks)
                {
					feedback.FeedbackProvider = new User()
					{
						Id = feedback.FeedbackProvider.Id,
						FirstName = feedback.FeedbackProvider.FirstName,
						LastName = feedback.FeedbackProvider.LastName,
						Email = feedback.FeedbackProvider.Email
					};
				}
            }

            return this;
        }
    }


	#region Meeting Insights

	public class Roleplay
	{
		public string Purpose { get; set; }
		public string Scenario { get; set; }
	}

	public class QuantitativeFeedback
	{
		public int Clarity { get; set; }
		public int Influence { get; set; }
		public int EmotionalIntelligence { get; set; }
		public int Listening { get; set; }
		public int ArticulateCommunication { get; set; }
	}

    public class QuantitativeFeedbackV2
    {
        public int ClarityOfCommunication { get; set; }
        public int ActiveListening { get; set; }
        public int EmotionalIntelligence { get; set; }
        public int Persuasiveness { get; set; }
        public int ProblemSolvingAndAdaptability { get; set; }
        public int ProfessionalismAndDecorum { get; set; }
        public int ImpactAndInfluence { get; set; }
        public int ArticulateCommunication { get; set; }
    }

	public class QuantitativeFeedbackV3
	{
		public int ClarityOfCommunication { get; set; }
		public int ActiveListening { get; set; }
		public int EmotionalIntelligence { get; set; }
		public int ProblemSolvingAndAdaptability { get; set; }
		public int ProfessionalismAndDecorum { get; set; }
		public int InfluentialCommunication { get; set; }
	}


	public class Insight
    {
		[JsonProperty("id")]
        public string Id { get; set; }
		[JsonProperty("userId")]
		public string UserId { get; set; }
        public string Date { get; set; }
        public string Source { get; set; }
        public string Title { get; set; }
        public string Markdown { get; set; }
        public string MarkdownV2 { get; set; }
		public string MarkdownV3 { get; set; }
		public Roleplay Roleplay {  get; set; }
        public Roleplay AlternateRoleplay { get; set; }
        public QuantitativeFeedback QuantitativeFeedback { get; set;}
        public QuantitativeFeedbackV2 QuantitativeFeedbackV2 { get; set; }
		public QuantitativeFeedbackV3 QuantitativeFeedbackV3 { get; set; }
	}

	#endregion
}