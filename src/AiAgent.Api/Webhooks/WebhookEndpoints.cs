using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AiAgent.Application.WhatsApp;
using AiAgent.Infrastructure.Persistence;
using AiAgent.Infrastructure.Tenancy;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AiAgent.Application.Interfaces;

namespace AiAgent.Api.Webhooks;

public static class WebhookEndpoints
{
    public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        // Meta webhook verification challenge
        app.MapGet("/webhook", (
            [FromQuery(Name = "hub.mode")] string? mode,
            [FromQuery(Name = "hub.verify_token")] string? verifyToken,
            [FromQuery(Name = "hub.challenge")] string? challenge,
            IConfiguration config) =>
        {
            var expectedToken = config["WhatsApp:VerifyToken"] ?? string.Empty;

            if (mode == "subscribe" &&
                !string.IsNullOrEmpty(verifyToken) &&
                verifyToken == expectedToken)
            {
                return Results.Ok(challenge);
            }

            return Results.Unauthorized();
        });

        // Inbound messages from Meta WhatsApp Cloud API
        app.MapPost("/webhook", async (
            HttpContext httpContext,
            AppDbContext db,
            RequestTenantContext tenantContext,
            IMediator mediator,
            ISecretEncryptionService encryption,
            IConfiguration config,
            CancellationToken ct) =>
        {
            // Read raw body for HMAC validation
            using var ms = new MemoryStream();
            await httpContext.Request.Body.CopyToAsync(ms, ct);
            var bodyBytes = ms.ToArray();

            // Validate Meta HMAC-SHA256 signature
            var appSecret = config["WhatsApp:AppSecret"] ?? string.Empty;
            if (!string.IsNullOrEmpty(appSecret))
            {
                var signatureHeader = httpContext.Request.Headers["X-Hub-Signature-256"].ToString();
                var provided = signatureHeader.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)
                    ? signatureHeader["sha256=".Length..]
                    : string.Empty;

                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appSecret));
                var computed = Convert.ToHexString(hmac.ComputeHash(bodyBytes)).ToLowerInvariant();

                if (!CryptographicOperations.FixedTimeEquals(
                        Encoding.UTF8.GetBytes(computed),
                        Encoding.UTF8.GetBytes(provided)))
                {
                    return Results.Unauthorized();
                }
            }

            var payload = JsonSerializer.Deserialize<WhatsAppPayload>(bodyBytes);

            // Return 200 immediately for anything that isn't a user message
            var change = payload?.Entry?.FirstOrDefault()
                ?.Changes?.FirstOrDefault(c => c.Field == "messages");
            var messages = change?.Value?.Messages;

            if (messages is null or { Count: 0 })
                return Results.Ok();

            var phoneNumberId = change!.Value.Metadata.PhoneNumberId;

            // Resolve tenant by WhatsApp phone number ID (Tenant is not tenant-scoped, no filter applied)
            var tenant = await db.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.WhatsAppPhoneNumberId == phoneNumberId, ct);

            if (tenant is null)
                return Results.Ok(); // Unknown phone number – acknowledge silently

            tenantContext.SetTenantId(tenant.Id);

            var decryptedToken = encryption.Decrypt(tenant.WhatsAppAccessToken);

            foreach (var msg in messages.Where(m => m.Type == "text" && m.Text is not null))
            {
                await mediator.Send(
                    new ProcessInboundMessageCommand(
                        tenant.Id,
                        msg.From,
                        msg.Text!.Body,
                        phoneNumberId,
                        decryptedToken),
                    ct);
            }

            return Results.Ok();
        });

        return app;
    }
}
