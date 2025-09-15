using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skill_Matrix.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseSuggestionColumnLengt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Suggestions",
                table: "Suggestions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "ResourceLink",
                table: "Suggestions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Suggestions",
                table: "Suggestions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "ResourceLink",
                table: "Suggestions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);
        }
    }
}
