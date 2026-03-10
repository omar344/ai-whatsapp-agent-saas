using AiAgent.Application.Interfaces;
using AiAgent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// DB Connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("AiAgent.Infrastructure")));

// Dummy Tenant Provider for now
builder.Services.AddScoped<ITenantProvider, DummyTenantProvider>();

var app = builder.Build();

app.MapOpenApi();
app.UseHttpsRedirection();

app.MapGet("/", () => new { Status = "AI Middleware Running" });

app.Run();

public class DummyTenantProvider : ITenantProvider { public Guid? TenantId => null; }