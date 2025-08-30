using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShadowrunGM.API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCharactersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "characters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Body = table.Column<int>(type: "integer", nullable: false),
                    Agility = table.Column<int>(type: "integer", nullable: false),
                    Reaction = table.Column<int>(type: "integer", nullable: false),
                    Strength = table.Column<int>(type: "integer", nullable: false),
                    Willpower = table.Column<int>(type: "integer", nullable: false),
                    Logic = table.Column<int>(type: "integer", nullable: false),
                    Intuition = table.Column<int>(type: "integer", nullable: false),
                    Charisma = table.Column<int>(type: "integer", nullable: false),
                    CurrentEdge = table.Column<int>(type: "integer", nullable: false),
                    MaxEdge = table.Column<int>(type: "integer", nullable: false),
                    PhysicalBoxes = table.Column<int>(type: "integer", nullable: false),
                    StunBoxes = table.Column<int>(type: "integer", nullable: false),
                    PhysicalDamage = table.Column<int>(type: "integer", nullable: false),
                    StunDamage = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_characters", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_characters_created_at",
                table: "characters",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_characters_modified_at",
                table: "characters",
                column: "modified_at");

            migrationBuilder.CreateIndex(
                name: "ix_characters_name",
                table: "characters",
                column: "name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "characters");
        }
    }
}
