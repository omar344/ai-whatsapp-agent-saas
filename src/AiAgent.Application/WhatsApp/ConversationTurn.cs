namespace AiAgent.Application.WhatsApp;

public sealed record ConversationTurn(string Role, string Content, DateTimeOffset Timestamp);
