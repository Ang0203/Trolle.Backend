using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trolle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLabels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tags",
                table: "card");

            migrationBuilder.CreateTable(
                name: "label",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    text_color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    board_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_label", x => x.id);
                    table.ForeignKey(
                        name: "FK_label_board_board_id",
                        column: x => x.board_id,
                        principalTable: "board",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "card_label",
                columns: table => new
                {
                    CardsId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabelsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_label", x => new { x.CardsId, x.LabelsId });
                    table.ForeignKey(
                        name: "FK_card_label_card_CardsId",
                        column: x => x.CardsId,
                        principalTable: "card",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_card_label_label_LabelsId",
                        column: x => x.LabelsId,
                        principalTable: "label",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_card_label_LabelsId",
                table: "card_label",
                column: "LabelsId");

            migrationBuilder.CreateIndex(
                name: "IX_label_board_id",
                table: "label",
                column: "board_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "card_label");

            migrationBuilder.DropTable(
                name: "label");

            migrationBuilder.AddColumn<string>(
                name: "tags",
                table: "card",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
