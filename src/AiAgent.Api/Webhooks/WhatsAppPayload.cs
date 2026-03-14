using System.Text.Json.Serialization;

namespace AiAgent.Api.Webhooks;

public sealed class WhatsAppPayload
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("entry")]
    public List<WhatsAppEntry> Entry { get; set; } = [];
}

public sealed class WhatsAppEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("changes")]
    public List<WhatsAppChange> Changes { get; set; } = [];
}

public sealed class WhatsAppChange
{
    [JsonPropertyName("value")]
    public WhatsAppChangeValue Value { get; set; } = new();

    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;
}

public sealed class WhatsAppChangeValue
{
    [JsonPropertyName("metadata")]
    public WhatsAppMetadata Metadata { get; set; } = new();

    [JsonPropertyName("messages")]
    public List<WhatsAppMessage>? Messages { get; set; }

    [JsonPropertyName("contacts")]
    public List<WhatsAppContact>? Contacts { get; set; }
}

public sealed class WhatsAppMetadata
{
    [JsonPropertyName("display_phone_number")]
    public string DisplayPhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("phone_number_id")]
    public string PhoneNumberId { get; set; } = string.Empty;
}

public sealed class WhatsAppMessage
{
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public WhatsAppTextBody? Text { get; set; }
}

public sealed class WhatsAppTextBody
{
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}

public sealed class WhatsAppContact
{
    [JsonPropertyName("profile")]
    public WhatsAppProfile Profile { get; set; } = new();

    [JsonPropertyName("wa_id")]
    public string WaId { get; set; } = string.Empty;
}

public sealed class WhatsAppProfile
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
