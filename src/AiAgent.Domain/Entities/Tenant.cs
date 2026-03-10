namespace AiAgent.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string WhatsAppPhoneNumberId { get; set; } = string.Empty; // Used for routing
    public string StoreConnectionString { get; set; } = string.Empty; // Encrypted in Milestone 2
}