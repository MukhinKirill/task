using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Task.Connector.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "TestTaskSchema");

            migrationBuilder.CreateTable(
                name: "ItRole",
                schema: "TestTaskSchema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    corporatePhoneNumber = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItRole", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Passwords",
                schema: "TestTaskSchema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userId = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passwords", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "RequestRight",
                schema: "TestTaskSchema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestRight", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "TestTaskSchema",
                columns: table => new
                {
                    login = table.Column<string>(type: "text", nullable: false),
                    lastName = table.Column<string>(type: "text", nullable: false),
                    firstName = table.Column<string>(type: "text", nullable: false),
                    middleName = table.Column<string>(type: "text", nullable: false),
                    telephoneNumber = table.Column<string>(type: "text", nullable: false),
                    isLead = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.login);
                });

            migrationBuilder.CreateTable(
                name: "UserITRole",
                schema: "TestTaskSchema",
                columns: table => new
                {
                    userId = table.Column<string>(type: "text", nullable: false),
                    roleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserITRole", x => new { x.roleId, x.userId });
                });

            migrationBuilder.CreateTable(
                name: "UserRequestRight",
                schema: "TestTaskSchema",
                columns: table => new
                {
                    userId = table.Column<string>(type: "text", nullable: false),
                    rightId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRequestRight", x => new { x.userId, x.rightId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItRole",
                schema: "TestTaskSchema");

            migrationBuilder.DropTable(
                name: "Passwords",
                schema: "TestTaskSchema");

            migrationBuilder.DropTable(
                name: "RequestRight",
                schema: "TestTaskSchema");

            migrationBuilder.DropTable(
                name: "User",
                schema: "TestTaskSchema");

            migrationBuilder.DropTable(
                name: "UserITRole",
                schema: "TestTaskSchema");

            migrationBuilder.DropTable(
                name: "UserRequestRight",
                schema: "TestTaskSchema");
        }
    }
}
