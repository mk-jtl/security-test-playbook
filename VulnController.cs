using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.AspNetCore.Mvc;

namespace SecurityDemo;

[ApiController]
[Route("api/[controller]")]
public class VulnController : ControllerBase
{
    private static readonly string DataDir = Path.GetFullPath("/data");

    // Path injection: keep only the file name, then check the result stays inside the data directory.
    [HttpGet("file")]
    public IActionResult ReadFile(string name)
    {
        var fileName = Path.GetFileName(name);
        var fullPath = Path.GetFullPath(Path.Combine(DataDir, fileName));
        if (!fullPath.StartsWith(DataDir + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            return BadRequest();
        }

        return Ok(System.IO.File.ReadAllText(fullPath));
    }

    // Command injection: no shell, and user input only selects from fixed values.
    [HttpGet("run")]
    public IActionResult Run(string arg)
    {
        var option = arg switch
        {
            "status" => "status",
            "version" => "--version",
            _ => null
        };
        if (option is null)
        {
            return BadRequest();
        }

        var startInfo = new ProcessStartInfo("git") { ArgumentList = { option } };
        using var process = Process.Start(startInfo);
        return Ok();
    }

    // Open redirect: only local URLs are allowed.
    [HttpGet("redirect")]
    public IActionResult Go(string url)
    {
        if (!Url.IsLocalUrl(url))
        {
            return BadRequest();
        }

        return LocalRedirect(url);
    }

    // XXE: DTD processing prohibited, no external resolver.
    [HttpPost("xml")]
    public IActionResult Xml(string xml)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };
        using var reader = XmlReader.Create(new StringReader(xml), settings);
        while (reader.Read())
        {
        }

        return Ok();
    }

    // Regex injection: the user input is escaped and a timeout is set.
    [HttpGet("regex")]
    public IActionResult Re(string pattern, string input)
    {
        var escaped = Regex.Escape(pattern);
        return Ok(Regex.IsMatch(input, escaped, RegexOptions.None, TimeSpan.FromSeconds(1)));
    }

    // Log forging: structured logging with line breaks removed from the user value.
    [HttpGet("log")]
    public IActionResult Log(string msg, [FromServices] ILogger<VulnController> logger)
    {
        var safe = msg.Replace("\r", string.Empty).Replace("\n", string.Empty);
        logger.LogInformation("User said: {Message}", safe);
        return Ok();
    }

    // Weak encryption: AES instead of DES.
    [HttpGet("crypt")]
    public IActionResult Crypt()
    {
        using var aes = Aes.Create();
        return Ok(aes.KeySize);
    }
}