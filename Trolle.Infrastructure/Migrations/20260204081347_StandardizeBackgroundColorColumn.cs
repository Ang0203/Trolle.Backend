using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trolle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeBackgroundColorColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BackgroundColor",
                table: "board",
                newName: "background_color");

            migrationBuilder.AlterColumn<string>(
                name: "background_color",
                table: "board",
                type: "text",
                nullable: false,
                defaultValue: "#1e293b",
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "background_color",
                table: "board",
                newName: "BackgroundColor");

            migrationBuilder.AlterColumn<string>(
                name: "BackgroundColor",
                table: "board",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "#1e293b");
        }
    }
}
