namespace StudyMate.Models
{
	public class SignInRequest
	{
		public required string Id { get; set; } // รหัสโทเค็นสุ่ม (Primary Key) เก็บในรูปแบบ varchar(64)
		public required string Password { get; set; } // รหัสนักศึกษา (Foreign Key) เก็บในรูปแบบ varchar(8)
	}
}
