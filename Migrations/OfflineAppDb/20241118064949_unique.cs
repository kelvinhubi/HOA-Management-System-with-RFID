using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Migrations.OfflineAppDb
{
    /// <inheritdoc />
    public partial class unique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "User_Accounts",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Guard_Information",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "FeesName",
                table: "feesLists",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_Information_PlateNo",
                table: "Vehicle_Information",
                column: "PlateNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Accounts_Username",
                table: "User_Accounts",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Guard_Information_Username",
                table: "Guard_Information",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_feesLists_FeesName",
                table: "feesLists",
                column: "FeesName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Due_Details_Invoice",
                table: "Due_Details",
                column: "Invoice",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicle_Information_PlateNo",
                table: "Vehicle_Information");

            migrationBuilder.DropIndex(
                name: "IX_User_Accounts_Username",
                table: "User_Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Guard_Information_Username",
                table: "Guard_Information");

            migrationBuilder.DropIndex(
                name: "IX_feesLists_FeesName",
                table: "feesLists");

            migrationBuilder.DropIndex(
                name: "IX_Due_Details_Invoice",
                table: "Due_Details");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "User_Accounts",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Guard_Information",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "FeesName",
                table: "feesLists",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
