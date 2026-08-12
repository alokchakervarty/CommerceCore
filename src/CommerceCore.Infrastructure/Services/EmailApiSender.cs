using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CommerceCore.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace CommerceCore.Infrastructure.Services;

public class EmailApiSender : IEmailApiSender
{
    private readonly HttpClient _httpClient;
    private readonly EmailApiSettings _settings;

    public EmailApiSender(HttpClient httpClient, IOptions<EmailApiSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.EmailAPIBaseURL) || string.IsNullOrWhiteSpace(_settings.EmailAPIKey))
        {
            throw new InvalidOperationException("Email API configuration is missing. Please set EmailAPIBaseURL and EmailAPIKey in appsettings.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, _settings.EmailAPIBaseURL);
        request.Version = HttpVersion.Version11;
        request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;

        request.Headers.UserAgent.ParseAdd("CommerceCore/1.0");
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.EmailAPIKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var payload = new
        {
            to = toAddress,
            subject,
            htmlBody
        };

        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Email API request failed with status {response.StatusCode}: {body}");
        }
    }
}
