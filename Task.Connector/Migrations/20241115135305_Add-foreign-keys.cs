using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Task.Connector.Migrations
{
    /// <inheritdoc />
    public partial class Addforeignkeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "TestTaskSchema");

            migrationBuilder.CreateTable(
                name: "_MigrationHistory",
                schema: "TestTaskSchema",
                columns: table => new
                {
                    MigrationId = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ProductVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MigrationHistory", x => x.MigrationId);
                });

            migrationBuilder.CreateTable(
                name: "ItRole",
                schema: "TestTaskSchema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    corporatePhoneNumber = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItRole", x => x.id);
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
                    login = table.Column<string>(type: "character varying(22)", maxLength: 22, nullable: false),
                    lastName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    firstName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    middleName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    telephoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    isLead = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.login);
                });

            migrationBuilder.CreateTable(
                name: "Passwords",
                schema: "TestTaskSchema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userId = table.Column<string>(type: "character varying(22)", maxLength: 22, nullable: false),
                    password = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passwords", x => x.id);
                    table.ForeignKey(
                        name: "FK_Passwords_User_userId",
                        column: x => x.userId,
                        principalSchema: "TestTaskSchema",
                        principalTable: "User",
                        principalColumn: "login",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserITRole",
                schema: "TestTaskSchema",
                columns: table => new
                {
                    userId = table.Column<string>(type: "character varying(22)", maxLength: 22, nullable: false),
                    roleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserITRole", x => new { x.roleId, x.userId });
                    table.ForeignKey(
                        name: "FK_UserITRole_ItRole_roleId",
                        column: x => x.roleId,
                        principalSchema: "TestTaskSchema",
                        principalTable: "ItRole",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserITRole_User_userId",
                        column: x => x.userId,
                        principalSchema: "TestTaskSchema",
                        principalTable: "User",
                        principalColumn: "login",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRequestRight",
                schema: "TestTaskSchema",
                columns: table => new
                {
                    userId = table.Column<string>(type: "character varying(22)", maxLength: 22, nullable: false),
                    rightId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRequestRight", x => new { x.rightId, x.userId });
                    table.ForeignKey(
                        name: "FK_UserRequestRight_RequestRight_rightId",
                        column: x => x.rightId,
                        principalSchema: "TestTaskSchema",
                        principalTable: "RequestRight",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRequestRight_User_userId",
                        column: x => x.userId,
                        principalSchema: "TestTaskSchema",
                        principalTable: "User",
                        principalColumn: "login",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Passwords_userId",
                schema: "TestTaskSchema",
                table: "Passwords",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_UserITRole_userId",
                schema: "TestTaskSchema",
                table: "UserITRole",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRequestRight_userId",
                schema: "TestTaskSchema",
                table: "UserRequestRight",
                column: "userId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_MigrationHistory",
                schema: "TestTaskSchema");

            migrationBuilder.DropTable(
                name: "Passwords",
                schema: "TestTaskSchema");

            migrationBuilder.DropTable(
                name: "UserITRole",
                schema: "TestTaskSchema");

            migrationBuilder.DropTable(
                name: "UserRequestRight",
                schema: "TestTaskSchema");

            migrationBuilder.DropTable(
                name: "ItRole",
                schema: "TestTaskSchema");

            migrationBuilder.DropTable(
                name: "RequestRight",
                schema: "TestTaskSchema");

            migrationBuilder.DropTable(
                name: "User",
                schema: "TestTaskSchema");
        }
    }
}
