using System.Linq.Expressions;
using AiAgent.Application.Interfaces;
using AiAgent.Domain.Common;
using AiAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiAgent.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // This enables the pgvector extension in Postgres
        modelBuilder.HasPostgresExtension("vector");

        // Apply tenant isolation filter to every entity implementing IMustHaveTenant.
        // When ITenantProvider.TenantId is null, EF translates this to 1=0, returning no rows.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IMustHaveTenant).IsAssignableFrom(entityType.ClrType)) continue;

            var param = Expression.Parameter(entityType.ClrType, "e");
            var tenantIdProp = Expression.Property(param, nameof(IMustHaveTenant.TenantId));
            var providerAccess = Expression.Property(
                Expression.Constant(_tenantProvider),
                typeof(ITenantProvider).GetProperty(nameof(ITenantProvider.TenantId))!);
            var body = Expression.Equal(
                Expression.Convert(tenantIdProp, typeof(Guid?)),
                providerAccess);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(body, param));
        }
    }
}