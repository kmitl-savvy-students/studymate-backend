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
        const string connectionString = "server=192.168.50.52;uid=admin;pwd=adminsdmkmitl;database=sdm-kmitl-db";
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
