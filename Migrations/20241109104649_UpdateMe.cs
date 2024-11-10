using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RFID_number",
                table: "Homeowner_Details");

            migrationBuilder.DropColumn(
                name: "ExitTime",
                table: "accessLogs");

            migrationBuilder.RenameColumn(
                name: "HomeID",
                table: "accessLogs",
                newName: "RFID_number");

            migrationBuilder.RenameColumn(
                name: "EntryTime",
                table: "accessLogs",
                newName: "Time");

            migrationBuilder.RenameColumn(
                name: "Comments",
                table: "accessLogs",
                newName: "LogType");

            migrationBuilder.AddColumn<int>(
                name: "AccountID",
                table: "accessLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountID",
                table: "accessLogs");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "accessLogs",
                newName: "EntryTime");

            migrationBuilder.RenameColumn(
                name: "RFID_number",
                table: "accessLogs",
                newName: "HomeID");

            migrationBuilder.RenameColumn(
                name: "LogType",
                table: "accessLogs",
                newName: "Comments");

            migrationBuilder.AddColumn<string>(
                name: "RFID_number",
                table: "Homeowner_Details",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExitTime",
                table: "accessLogs",
                type: "datetime(6)",
                nullable: true);
        }
    }
}
