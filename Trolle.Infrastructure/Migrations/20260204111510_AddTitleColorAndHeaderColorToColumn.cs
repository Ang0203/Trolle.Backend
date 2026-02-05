using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trolle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleColorAndHeaderColorToColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "color",
                table: "column",
                newName: "title_color");

            migrationBuilder.AddColumn<string>(
                name: "header_color",
                table: "column",
                type: "text",
                nullable: false,
                defaultValue: "transparent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "header_color",
                table: "column");

            migrationBuilder.RenameColumn(
                name: "title_color",
                table: "column",
                newName: "color");
        }
    }
}
