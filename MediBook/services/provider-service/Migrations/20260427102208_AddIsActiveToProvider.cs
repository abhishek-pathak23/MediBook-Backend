using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace provider_service.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Providers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Providers");
        }
    }
}
