using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBookingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCustomerNameAndAddUserIdAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          // 1) Fjern CustomerName-kolonnen
    migrationBuilder.DropColumn(
        name: "CustomerName",
        table: "Reservations");

    // 2) Legg til UserId
    migrationBuilder.AddColumn<string>(
        name: "UserId",
        table: "Reservations",
        type: "nvarchar(450)",
        nullable: false,
        defaultValue: "");

    // 3) (Valgfritt) Opprett indeks for å gjøre spørringer på UserId raskere
    migrationBuilder.CreateIndex(
        name: "IX_Reservations_UserId",
        table: "Reservations",
        column: "UserId");

    // 4) Legg på FK til AspNetUsers, hvis du ønsker referanseintegritet
    migrationBuilder.AddForeignKey(
        name: "FK_Reservations_AspNetUsers_UserId",
        table: "Reservations",
        column: "UserId",
        principalTable: "AspNetUsers",
        principalColumn: "Id",
        onDelete: ReferentialAction.Cascade);

    // 5) Legg til Status-kolonnen
    migrationBuilder.AddColumn<int>(
        name: "Status",
        table: "Reservations",
        type: "int",
        nullable: false,
        defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
        name: "FK_Reservations_AspNetUsers_UserId",
        table: "Reservations");

    migrationBuilder.DropIndex(
        name: "IX_Reservations_UserId",
        table: "Reservations");

    migrationBuilder.DropColumn(
        name: "UserId",
        table: "Reservations");

    migrationBuilder.DropColumn(
        name: "Status",
        table: "Reservations");

    // Legg tilbake CustomerName-kolonnen slik den var:
    migrationBuilder.AddColumn<string>(
        name: "CustomerName",
        table: "Reservations",
        type: "nvarchar(200)",
        maxLength: 200,
        nullable: false,
        defaultValue: "");
        }
    }
}
