using AiAgent.Application.Interfaces;

namespace AiAgent.Infrastructure.Tenancy;

/// <summary>
/// Scoped per-request store for the resolved tenant identity.
/// Set early in the webhook pipeline; read transparently by AppDbContext via ITenantProvider.
/// </summary>
public sealed class RequestTenantContext : ITenantProvider
{
    private Guid? _tenantId;

    public Guid? TenantId => _tenantId;

    public void SetTenantId(Guid tenantId) => _tenantId = tenantId;
}
