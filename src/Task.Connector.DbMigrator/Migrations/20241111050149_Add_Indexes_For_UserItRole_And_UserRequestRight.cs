using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task.Connector.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Add_Indexes_For_UserItRole_And_UserRequestRight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_user_request_right_user_id_right_id",
                schema: "task",
                table: "user_request_right",
                columns: new[] { "user_id", "right_id" });

            migrationBuilder.CreateIndex(
                name: "IX_user_it_role_user_id_role_id",
                schema: "task",
                table: "user_it_role",
                columns: new[] { "user_id", "role_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_request_right_user_id_right_id",
                schema: "task",
                table: "user_request_right");

            migrationBuilder.DropIndex(
                name: "IX_user_it_role_user_id_role_id",
                schema: "task",
                table: "user_it_role");
        }
    }
}
