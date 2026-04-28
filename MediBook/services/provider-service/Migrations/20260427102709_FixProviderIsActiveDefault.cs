using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace provider_service.Migrations
{
    /// <inheritdoc />
    public partial class FixProviderIsActiveDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix: previous migration defaulted IsActive to 0 (false) for all existing rows.
            // Set all existing providers to active.
            migrationBuilder.Sql("UPDATE [Providers] SET [IsActive] = 1 WHERE [IsActive] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
