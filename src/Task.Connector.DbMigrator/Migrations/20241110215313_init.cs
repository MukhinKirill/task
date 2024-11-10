using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Task.Connector.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "task");

            migrationBuilder.CreateTable(
                name: "it_role",
                schema: "task",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "varchar", maxLength: 100, nullable: false),
                    corporate_phone_number = table.Column<string>(type: "varchar", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_it_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "passwords",
                schema: "task",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "varchar", maxLength: 22, nullable: false),
                    password = table.Column<string>(type: "varchar", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_passwords", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "request_right",
                schema: "task",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_request_right", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_it_role",
                schema: "task",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "varchar", maxLength: 22, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_it_role", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "user_request_right",
                schema: "task",
                columns: table => new
                {
                    right_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "varchar", maxLength: 22, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_request_right", x => x.right_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "task",
                columns: table => new
                {
                    login = table.Column<string>(type: "varchar", maxLength: 22, nullable: false),
                    last_name = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    first_name = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    middle_name = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    telephone_number = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    is_lead = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.login);
                });

            migrationBuilder.CreateIndex(
                name: "IX_it_role_id",
                schema: "task",
                table: "it_role",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_passwords_id",
                schema: "task",
                table: "passwords",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_passwords_user_id",
                schema: "task",
                table: "passwords",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_request_right_id",
                schema: "task",
                table: "request_right",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_user_it_role_role_id",
                schema: "task",
                table: "user_it_role",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_it_role_user_id",
                schema: "task",
                table: "user_it_role",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_request_right_right_id",
                schema: "task",
                table: "user_request_right",
                column: "right_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_request_right_user_id",
                schema: "task",
                table: "user_request_right",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_login",
                schema: "task",
                table: "users",
                column: "login");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "it_role",
                schema: "task");

            migrationBuilder.DropTable(
                name: "passwords",
                schema: "task");

            migrationBuilder.DropTable(
                name: "request_right",
                schema: "task");

            migrationBuilder.DropTable(
                name: "user_it_role",
                schema: "task");

            migrationBuilder.DropTable(
                name: "user_request_right",
                schema: "task");

            migrationBuilder.DropTable(
                name: "users",
                schema: "task");
        }
    }
}
