using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyMate.Migrations
{
    /// <inheritdoc />
    public partial class AddGenderColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "user_management");

            migrationBuilder.CreateTable(
                name: "user",
                schema: "user_management",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    NameNick = table.Column<string>(type: "text", nullable: true),
                    NameFirst = table.Column<string>(type: "text", nullable: true),
                    NameLast = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user",
                schema: "user_management");
        }
    }
}
