using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class FirstBaseModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequestRights",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestRights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Login = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TelephoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsLead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Login);
                });

            migrationBuilder.CreateTable(
                name: "Passwords",
                columns: table => new
                {
                    UserLogin = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passwords", x => x.UserLogin);
                    table.ForeignKey(
                        name: "FK_Passwords_Users_UserLogin",
                        column: x => x.UserLogin,
                        principalTable: "Users",
                        principalColumn: "Login",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserItRoles",
                columns: table => new
                {
                    UserLogin = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ItRoleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserItRoles", x => new { x.ItRoleId, x.UserLogin });
                    table.ForeignKey(
                        name: "FK_UserItRoles_ItRoles_ItRoleId",
                        column: x => x.ItRoleId,
                        principalTable: "ItRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserItRoles_Users_UserLogin",
                        column: x => x.UserLogin,
                        principalTable: "Users",
                        principalColumn: "Login",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRequestRights",
                columns: table => new
                {
                    UserLogin = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RequestRightId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRequestRights", x => new { x.UserLogin, x.RequestRightId });
                    table.ForeignKey(
                        name: "FK_UserRequestRights_RequestRights_RequestRightId",
                        column: x => x.RequestRightId,
                        principalTable: "RequestRights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRequestRights_Users_UserLogin",
                        column: x => x.UserLogin,
                        principalTable: "Users",
                        principalColumn: "Login",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserItRoles_UserLogin",
                table: "UserItRoles",
                column: "UserLogin");

            migrationBuilder.CreateIndex(
                name: "IX_UserRequestRights_RequestRightId",
                table: "UserRequestRights",
                column: "RequestRightId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Passwords");

            migrationBuilder.DropTable(
                name: "UserItRoles");

            migrationBuilder.DropTable(
                name: "UserRequestRights");

            migrationBuilder.DropTable(
                name: "ItRoles");

            migrationBuilder.DropTable(
                name: "RequestRights");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
