using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;

#nullable disable

namespace ShadowrunGM.API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,")
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "sourcebooks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    edition = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "6e"),
                    file_name = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    file_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: true),
                    created_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    updated_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    imported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sourcebooks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "game_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    item_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cost = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    availability = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    stats_json = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    sourcebook_id = table.Column<long>(type: "bigint", nullable: false),
                    page = table.Column<int>(type: "integer", nullable: true),
                    embedding = table.Column<Vector>(type: "vector(768)", nullable: true),
                    embedding_model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    embedding_version = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_game_items_sourcebooks_sourcebook_id",
                        column: x => x.sourcebook_id,
                        principalTable: "sourcebooks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "magic_abilities",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    ability_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    range = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    duration = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    drain_value = table.Column<int>(type: "integer", nullable: true),
                    power_point_cost = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    extra_json = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    sourcebook_id = table.Column<long>(type: "bigint", nullable: false),
                    page = table.Column<int>(type: "integer", nullable: true),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    embedding = table.Column<Vector>(type: "vector(768)", nullable: true),
                    embedding_model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    embedding_version = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_magic_abilities", x => x.id);
                    table.ForeignKey(
                        name: "fk_magic_abilities_sourcebooks_sourcebook_id",
                        column: x => x.sourcebook_id,
                        principalTable: "sourcebooks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rule_contents",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sourcebook_id = table.Column<long>(type: "bigint", nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false),
                    page_number = table.Column<int>(type: "integer", nullable: true),
                    parent_content_id = table.Column<long>(type: "bigint", nullable: true),
                    heading = table.Column<string>(type: "text", nullable: true),
                    heading_title = table.Column<string>(type: "text", nullable: true),
                    heading_level = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    section = table.Column<string>(type: "text", nullable: true),
                    content_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    content = table.Column<string>(type: "text", nullable: false),
                    source_hash = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, computedColumnSql: "md5(content)", stored: true),
                    embedding = table.Column<Vector>(type: "vector(768)", nullable: true),
                    embedding_model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    embedding_version = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rule_contents", x => x.id);
                    table.ForeignKey(
                        name: "fk_rule_contents_rule_contents_parent_content_id",
                        column: x => x.parent_content_id,
                        principalTable: "rule_contents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_rule_contents_sourcebooks_sourcebook_id",
                        column: x => x.sourcebook_id,
                        principalTable: "sourcebooks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_game_items_cost",
                table: "game_items",
                column: "cost");

            migrationBuilder.CreateIndex(
                name: "ix_game_items_item_type_category",
                table: "game_items",
                columns: new[] { "item_type", "category" });

            migrationBuilder.CreateIndex(
                name: "ix_game_items_slug",
                table: "game_items",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_game_items_sourcebook_id",
                table: "game_items",
                column: "sourcebook_id");

            migrationBuilder.CreateIndex(
                name: "ix_game_items_sourcebook_id_page",
                table: "game_items",
                columns: new[] { "sourcebook_id", "page" });

            migrationBuilder.CreateIndex(
                name: "ix_magic_abilities_ability_type_category",
                table: "magic_abilities",
                columns: new[] { "ability_type", "category" });

            migrationBuilder.CreateIndex(
                name: "ix_magic_abilities_is_verified",
                table: "magic_abilities",
                column: "is_verified");

            migrationBuilder.CreateIndex(
                name: "ix_magic_abilities_slug",
                table: "magic_abilities",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_magic_abilities_sourcebook_id",
                table: "magic_abilities",
                column: "sourcebook_id");

            migrationBuilder.CreateIndex(
                name: "ix_magic_abilities_sourcebook_id_page",
                table: "magic_abilities",
                columns: new[] { "sourcebook_id", "page" });

            migrationBuilder.CreateIndex(
                name: "ix_rule_contents_content",
                table: "rule_contents",
                column: "content")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_rule_contents_heading",
                table: "rule_contents",
                column: "heading")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_rule_contents_heading_title",
                table: "rule_contents",
                column: "heading_title")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_rule_contents_parent_content_id",
                table: "rule_contents",
                column: "parent_content_id");

            migrationBuilder.CreateIndex(
                name: "ix_rule_contents_sourcebook_id_content_type",
                table: "rule_contents",
                columns: new[] { "sourcebook_id", "content_type" });

            migrationBuilder.CreateIndex(
                name: "ix_rule_contents_sourcebook_id_heading_level",
                table: "rule_contents",
                columns: new[] { "sourcebook_id", "heading_level" });

            migrationBuilder.CreateIndex(
                name: "ix_rule_contents_sourcebook_id_page_number",
                table: "rule_contents",
                columns: new[] { "sourcebook_id", "page_number" });

            migrationBuilder.CreateIndex(
                name: "ix_rule_contents_sourcebook_id_sequence_number",
                table: "rule_contents",
                columns: new[] { "sourcebook_id", "sequence_number" });

            migrationBuilder.CreateIndex(
                name: "ix_rule_contents_sourcebook_id_source_hash",
                table: "rule_contents",
                columns: new[] { "sourcebook_id", "source_hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sourcebooks_code",
                table: "sourcebooks",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sourcebooks_file_hash",
                table: "sourcebooks",
                column: "file_hash");

            migrationBuilder.CreateIndex(
                name: "ix_sourcebooks_title",
                table: "sourcebooks",
                column: "title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_items");

            migrationBuilder.DropTable(
                name: "magic_abilities");

            migrationBuilder.DropTable(
                name: "rule_contents");

            migrationBuilder.DropTable(
                name: "sourcebooks");
        }
    }
}
