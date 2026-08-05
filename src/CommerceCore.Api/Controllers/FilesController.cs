using Asp.Versioning;
using FluentFTP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;

namespace CommerceCore.Api.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/files")]
public class FilesController : ControllerBase
{
    private readonly FtpSettings _settings;

    public FilesController(IOptions<FtpSettings> settings)
    {
        _settings = settings.Value;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest();

        var extension = Path.GetExtension(file.FileName);

        var fileName = $"{Guid.NewGuid():N}{extension}";

        using var client = new AsyncFtpClient(
            _settings.Host,
            _settings.Username,
            _settings.Password);

        await client.Connect();

        using var stream = file.OpenReadStream();

        await client.UploadStream(
            stream,
            $"{_settings.RemoteFolder}/{fileName}",
            FtpRemoteExists.Overwrite,
            true);

        await client.Disconnect();

        return Ok(new
        {
            Url = $"{_settings.BaseUrl}/{fileName}"
        });
    }
}
public class FtpSettings
{
    public string Host { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string BaseUrl { get; set; } = "";
    public string RemoteFolder { get; set; } = "";
}