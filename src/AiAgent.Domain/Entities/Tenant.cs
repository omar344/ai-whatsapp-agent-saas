namespace AiAgent.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string WhatsAppPhoneNumberId { get; set; } = string.Empty;
    public string StoreConnectionString { get; set; } = string.Empty; // AES-GCM encrypted
    public string WhatsAppAccessToken { get; set; } = string.Empty;   // AES-GCM encrypted
    public string ApiKeyHash { get; set; } = string.Empty;            // SHA-256 hex of raw API key
}