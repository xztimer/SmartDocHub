using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHub_KMS.Core.Migrations
{
    /// <inheritdoc />
    public partial class update0803 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Permissions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Permissions");
        }
    }
}
