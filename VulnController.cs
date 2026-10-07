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
    // Path injection
    [HttpGet("file")]
    public IActionResult ReadFile(string name)
        => Ok(System.IO.File.ReadAllText("/data/" + name));

    // Command injection
    [HttpGet("run")]
    public IActionResult Run(string arg)
    {
        Process.Start("/bin/sh", "-c " + arg);
        return Ok();
    }

    // Open redirect
    [HttpGet("redirect")]
    public IActionResult Go(string url) => Redirect(url);

    // XXE
    [HttpPost("xml")]
    public IActionResult Xml(string xml)
    {
        var doc = new XmlDocument { XmlResolver = new XmlUrlResolver() };
        doc.LoadXml(xml);
        return Ok();
    }

    // Regex injection
    [HttpGet("regex")]
    public IActionResult Re(string pattern, string input)
        => Ok(Regex.IsMatch(input, pattern));

    // Log forging
    [HttpGet("log")]
    public IActionResult Log(string msg, [FromServices] ILogger<VulnController> logger)
    {
        logger.LogInformation("User said: " + msg);
        return Ok();
    }

    // Weak encryption
    [HttpGet("crypt")]
    public IActionResult Crypt()
    {
        using var des = DES.Create();
        return Ok(des.Key.Length);
    }
}