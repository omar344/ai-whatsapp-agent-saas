using AiAgent.Application.Interfaces;
using AiAgent.Infrastructure.Security;
using AiAgent.Infrastructure.Tenancy;
using AiAgent.Infrastructure.WhatsApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiAgent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Encryption (singleton – stateless, key loaded once)
        services.AddSingleton<ISecretEncryptionService, AesGcmEncryptionService>();

        // Tenant context: same scoped instance satisfies both ITenantProvider and RequestTenantContext
        services.AddScoped<RequestTenantContext>();
        services.AddScoped<ITenantProvider>(sp => sp.GetRequiredService<RequestTenantContext>());

        // Redis-backed conversation session store
        services.AddStackExchangeRedisCache(opts =>
            opts.Configuration = configuration["Redis:ConnectionString"]);
        services.AddScoped<IConversationSessionStore, ConversationSessionStore>();

        // WhatsApp Cloud API HTTP client
        services.AddHttpClient("whatsapp", client =>
            client.BaseAddress = new Uri("https://graph.facebook.com"));
        services.AddScoped<IWhatsAppSender, WhatsAppSender>();

        return services;
    }
}
