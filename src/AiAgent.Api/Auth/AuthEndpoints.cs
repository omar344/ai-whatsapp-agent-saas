using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AiAgent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AiAgent.Api.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/token", async (
            TokenRequest request,
            AppDbContext db,
            IConfiguration config,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.ApiKey))
                return Results.BadRequest(new { error = "apiKey is required." });

            var keyHash = ComputeSha256Hex(request.ApiKey);

            var tenant = await db.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ApiKeyHash == keyHash, ct);

            if (tenant is null)
                return Results.Unauthorized();

            var jwtKey = config["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is not configured.");
            var issuer = config["Jwt:Issuer"] ?? "ai-whatsapp-agent";
            var audience = config["Jwt:Audience"] ?? "dashboard";
            var expiryHours = config.GetValue("Jwt:ExpiryHours", 8);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var expiry = DateTime.UtcNow.AddHours(expiryHours);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims:
                [
                    new Claim("tenantId", tenant.Id.ToString()),
                    new Claim("tenantName", tenant.Name)
                ],
                expires: expiry,
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Results.Ok(new TokenResponse(tokenString, expiry.ToString("o")));
        });

        return app;
    }

    private static string ComputeSha256Hex(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

public sealed record TokenRequest(string ApiKey);
public sealed record TokenResponse(string Token, string ExpiresAt);
