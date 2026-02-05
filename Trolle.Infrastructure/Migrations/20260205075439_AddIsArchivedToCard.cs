using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trolle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsArchivedToCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                table: "card",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_archived",
                table: "card");
        }
    }
}
