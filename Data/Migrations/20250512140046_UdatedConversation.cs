using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UdatedConversation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Initialized",
                schema: "chat",
                table: "Conversations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Initialized",
                schema: "chat",
                table: "Conversations");
        }
    }
}
