using System.Text.Json;
using AiAgent.Application.Interfaces;
using AiAgent.Application.WhatsApp;
using Microsoft.Extensions.Caching.Distributed;

namespace AiAgent.Infrastructure.WhatsApp;

internal sealed class ConversationSessionStore(IDistributedCache cache) : IConversationSessionStore
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromHours(24)
    };

    public async Task<IReadOnlyList<ConversationTurn>> GetHistoryAsync(
        string senderPhoneNumber,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var json = await cache.GetStringAsync(CacheKey(tenantId, senderPhoneNumber), ct);
        if (string.IsNullOrEmpty(json))
            return [];

        return JsonSerializer.Deserialize<List<ConversationTurn>>(json) ?? [];
    }

    public async Task AppendTurnAsync(
        string senderPhoneNumber,
        Guid tenantId,
        string role,
        string content,
        CancellationToken ct = default)
    {
        var history = new List<ConversationTurn>(
            await GetHistoryAsync(senderPhoneNumber, tenantId, ct))
        {
            new(role, content, DateTimeOffset.UtcNow)
        };

        var json = JsonSerializer.Serialize(history);
        await cache.SetStringAsync(CacheKey(tenantId, senderPhoneNumber), json, CacheOptions, ct);
    }

    private static string CacheKey(Guid tenantId, string senderPhoneNumber) =>
        $"conv:{tenantId}:{senderPhoneNumber}";
}
