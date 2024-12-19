namespace DataAccess
{
	public class ReportDTO
	{
		public string Id { get; set; }
		public string Date { get; set; }
		public InteractionStatsDTO InteractionStats { get; set; }
		public string ModuleAssignmentId { get; set; }
		public string ModuleId { get; set; }
		public string ModuleTitle { get; set; }
		public string CoachId { get; set; }
		public string CoachFirstName { get; set; }
		public string CoachLastName { get; set; }
		public string CoachEmail { get; set; }
		public string LearnerId { get; set; }
		public string LearnerFirstName { get; set; }
		public string LearnerLastName { get; set; }
		public string LearnerEmail { get; set; }
		public ReportDTO() 
		{
			InteractionStats = new InteractionStatsDTO();
		}
	}

	// Assuming InteractionStats is a complex type, we need a DTO for it as well
	public class InteractionStatsDTO
	{
		public string InteractionStartTime { get; set; }
		public string InteractionEndTime { get; set; }
		public double RoleplayMinutes { get; set; }

		public InteractionStatsDTO() { }
	}

	public class ModuleAssignmentDTO
	{
		public string Id { get; set; }
		public ModuleDTO Module { get; set; }
		public CoachDTO Coach { get; set; }
		public LearnerDTO Learner{ get; set; }

		public ModuleAssignmentDTO() { }
	}

	public class ModuleDTO
	{
		public string Id { get; set; }
		public string Title { get; set; }

		public ModuleDTO() { }
	}

	public class CoachDTO
	{
		public string Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }

		public CoachDTO() { }
	}

	public class LearnerDTO
	{
		public string Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }

		public LearnerDTO() { }
	}
}