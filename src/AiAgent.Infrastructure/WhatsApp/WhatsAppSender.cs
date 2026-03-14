using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AiAgent.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiAgent.Infrastructure.WhatsApp;

internal sealed class WhatsAppSender(
    IHttpClientFactory httpClientFactory,
    ILogger<WhatsAppSender> logger)
    : IWhatsAppSender
{
    public async Task SendTextMessageAsync(
        string toPhoneNumber,
        string fromPhoneNumberId,
        string decryptedAccessToken,
        string message,
        CancellationToken ct = default)
    {
        var client = httpClientFactory.CreateClient("whatsapp");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", decryptedAccessToken);

        var payload = new
        {
            messaging_product = "whatsapp",
            to = toPhoneNumber,
            type = "text",
            text = new { body = message }
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(
            $"/v22.0/{fromPhoneNumberId}/messages", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogError(
                "WhatsApp send failed ({Status}): {Body}",
                response.StatusCode, body);
        }
    }
}
