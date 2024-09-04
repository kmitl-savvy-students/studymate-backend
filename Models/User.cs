namespace StudyMate.Models
{
	public class User
	{
		public required string Id { get; set; } // PK, corresponds to varchar(8)
		public required string Password { get; set; } // corresponds to varchar(64)
		public string? Gender { get; set; } // enum('MALE', 'FEMALE')
		public string? NameNick { get; set; } // corresponds to varchar(256)
		public string? NameFirst { get; set; } // corresponds to varchar(256)
		public string? NameLast { get; set; } // corresponds to varchar(256)
	}
}
