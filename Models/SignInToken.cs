namespace StudyMate.Models
{
	public class SigninToken
	{
		public required string Id { get; set; } // รหัสโทเค็นสุ่ม (Primary Key) เก็บในรูปแบบ varchar(64)
		public required string UserId { get; set; } // รหัสนักศึกษา (Foreign Key) เก็บในรูปแบบ varchar(8)
		public required DateTime TimeCreated { get; set; } // วันที่และเวลาที่สร้างโทเค็น
		public required DateTime TimeExpired { get; set; } // วันที่และเวลาที่โทเค็นหมดอายุ

		// คุณสมบัติการนำทางไปยัง User
		public User? User { get; set; }
	}
}
