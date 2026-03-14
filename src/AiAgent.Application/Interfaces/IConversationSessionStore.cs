using AiAgent.Application.WhatsApp;

namespace AiAgent.Application.Interfaces;

public interface IConversationSessionStore
{
    Task<IReadOnlyList<ConversationTurn>> GetHistoryAsync(
        string senderPhoneNumber,
        Guid tenantId,
        CancellationToken ct = default);

    Task AppendTurnAsync(
        string senderPhoneNumber,
        Guid tenantId,
        string role,
        string content,
        CancellationToken ct = default);
}
