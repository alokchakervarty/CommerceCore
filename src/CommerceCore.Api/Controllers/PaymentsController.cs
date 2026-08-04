using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommerceCore.Api.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api")]
public class PaymentsController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public PaymentsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateRazorpayOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.Amount < 100)
        {
            return BadRequest(new { message = "Amount must be at least 100 paise." });
        }

        var keyId = _configuration["RAZORPAY_KEY_ID"];
        var keySecret = _configuration["RAZORPAY_KEY_SECRET"];

        if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
        {
            return StatusCode(500, new { message = "Razorpay credentials are not configured on server." });
        }

        var client = _httpClientFactory.CreateClient();
        var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

        var payload = JsonSerializer.Serialize(new
        {
            amount = request.Amount,
            currency = string.IsNullOrWhiteSpace(request.Currency) ? "INR" : request.Currency,
            receipt = string.IsNullOrWhiteSpace(request.Receipt) ? $"receipt_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}" : request.Receipt,
        });

        using var response = await client.PostAsync(
            "https://api.razorpay.com/v1/orders",
            new StringContent(payload, Encoding.UTF8, "application/json"),
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return StatusCode(401, new { message = "Razorpay authentication failed." });
        }

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode(500, new { message = "Failed to create Razorpay order.", details = body });
        }

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        return Ok(new
        {
            order_id = root.GetProperty("id").GetString(),
            amount = root.GetProperty("amount").GetInt32(),
            currency = root.GetProperty("currency").GetString(),
        });
    }

    [HttpPost("verify-payment")]
    public IActionResult VerifyPayment([FromBody] VerifyRazorpayPaymentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RazorpayOrderId)
            || string.IsNullOrWhiteSpace(request.RazorpayPaymentId)
            || string.IsNullOrWhiteSpace(request.RazorpaySignature))
        {
            return BadRequest(new { message = "Missing required payment verification fields." });
        }

        var keySecret = _configuration["RAZORPAY_KEY_SECRET"];
        if (string.IsNullOrWhiteSpace(keySecret))
        {
            return StatusCode(500, new { message = "Razorpay secret is not configured on server." });
        }

        var data = $"{request.RazorpayOrderId}|{request.RazorpayPaymentId}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keySecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        var generatedSignature = Convert.ToHexString(hash).ToLowerInvariant();

        var provided = request.RazorpaySignature.Trim().ToLowerInvariant();
        var matched = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(generatedSignature),
            Encoding.UTF8.GetBytes(provided));

        if (!matched)
        {
            return BadRequest(new { success = false, message = "Payment signature mismatch." });
        }

        return Ok(new { success = true, message = "Payment verified successfully." });
    }
}

public record CreateRazorpayOrderRequest(int Amount, string Currency, string? Receipt);

public record VerifyRazorpayPaymentRequest(
    [property: JsonPropertyName("razorpay_order_id")] string RazorpayOrderId,
    [property: JsonPropertyName("razorpay_payment_id")] string RazorpayPaymentId,
    [property: JsonPropertyName("razorpay_signature")] string RazorpaySignature);
