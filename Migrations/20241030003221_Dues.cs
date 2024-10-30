using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Migrations
{
    /// <inheritdoc />
    public partial class Dues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "payID",
                table: "Due_Details",
                newName: "DueID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DueID",
                table: "Due_Details",
                newName: "payID");
        }
    }
}
