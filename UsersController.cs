using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace SecurityDemo;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{name}")]
    public IActionResult Get(string name)
    {
        using var conn = new SqlConnection("Server=.;Database=demo;");
        conn.Open();

        // SQL injection
        var cmd = new SqlCommand(
            "SELECT * FROM Users WHERE Name = '" + name + "'", conn);
        using var reader = cmd.ExecuteReader();
        return Ok();
    }

    [HttpGet("search")]
    public IActionResult Search(string q)
    {
        // Reflected XSS
        return Content("<h1>Results for " + q + "</h1>", "text/html");
    }
}