using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsClubApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRejectedFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRejected",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRejected",
                table: "Users");
        }
    }
}
