using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class FirstTestBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "TestTaskSchema");

            migrationBuilder.CreateTable(
                name: "ITRole",
                schema: "TestTaskSchema",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    corporatePhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ITRole", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "RequestRight",
                schema: "TestTaskSchema",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    login = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    lastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    firstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    middleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    telephoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isLead = table.Column<bool>(type: "bit", nullable: false)
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
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    userId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    roleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserITRole", x => new { x.roleId, x.userId });
                    table.ForeignKey(
                        name: "FK_UserITRole_ITRole_roleId",
                        column: x => x.roleId,
                        principalSchema: "TestTaskSchema",
                        principalTable: "ITRole",
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
                    userId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    rightId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRequestRight", x => new { x.userId, x.rightId });
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
                column: "userId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserITRole_userId",
                schema: "TestTaskSchema",
                table: "UserITRole",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRequestRight_rightId",
                schema: "TestTaskSchema",
                table: "UserRequestRight",
                column: "rightId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "ITRole",
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
