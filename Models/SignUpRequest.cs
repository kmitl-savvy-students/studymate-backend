namespace StudyMate.Models
{
	public class SignUpRequest
	{
		public required string Id { get; set; }
		public required string Password { get; set; }
		public required string PasswordConfirm { get; set; }
		public required string Gender { get; set; }
		public required string NameNick { get; set; }
		public required string NameFirst { get; set; }
		public required string NameLast { get; set; }

		public bool IsValidRequest()
		{
			if (Password != PasswordConfirm)
			{
				return false;
			}

			return string.IsNullOrWhiteSpace(Id)
			       || string.IsNullOrWhiteSpace(Password)
			       || string.IsNullOrWhiteSpace(Gender)
			       || string.IsNullOrWhiteSpace(NameFirst)
			       || string.IsNullOrWhiteSpace(NameLast)
			       || string.IsNullOrWhiteSpace(NameNick);
		}
	}
}
