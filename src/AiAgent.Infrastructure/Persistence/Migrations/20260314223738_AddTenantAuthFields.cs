using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiAgent.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantAuthFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKeyHash",
                table: "Tenants",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppAccessToken",
                table: "Tenants",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKeyHash",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "WhatsAppAccessToken",
                table: "Tenants");
        }
    }
}
