namespace AiAgent.Application.Interfaces;

public interface ITenantProvider
{
    Guid? TenantId { get; }
}