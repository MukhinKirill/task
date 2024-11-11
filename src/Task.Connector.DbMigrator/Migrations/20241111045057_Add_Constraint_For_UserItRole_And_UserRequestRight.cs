using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Task.Connector.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Add_Constraint_For_UserItRole_And_UserRequestRight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_request_right",
                schema: "task",
                table: "user_request_right");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_it_role",
                schema: "task",
                table: "user_it_role");

            migrationBuilder.AlterColumn<int>(
                name: "right_id",
                schema: "task",
                table: "user_request_right",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "role_id",
                schema: "task",
                table: "user_it_role",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_request_right",
                schema: "task",
                table: "user_request_right",
                columns: new[] { "user_id", "right_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_it_role",
                schema: "task",
                table: "user_it_role",
                columns: new[] { "user_id", "role_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_request_right",
                schema: "task",
                table: "user_request_right");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_it_role",
                schema: "task",
                table: "user_it_role");

            migrationBuilder.AlterColumn<int>(
                name: "right_id",
                schema: "task",
                table: "user_request_right",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "role_id",
                schema: "task",
                table: "user_it_role",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_request_right",
                schema: "task",
                table: "user_request_right",
                column: "right_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_it_role",
                schema: "task",
                table: "user_it_role",
                column: "role_id");
        }
    }
}
