using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    [HttpGet("test-connection")]
    public IActionResult TestConnection()
    {
        const string connectionString = "server=127.0.0.1;uid=admin;pwd=admin;database=studymate";
        try
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            return Ok("Connection successful!");
        }
        catch (MySqlException ex)
        {
            return StatusCode(500, $"Database connection failed: {ex.Message}");
        }
    }
}
