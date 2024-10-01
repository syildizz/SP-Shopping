using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SP_Shopping.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedDateTimeToOrderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InsertionDate",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModificationDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InsertionDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ModificationDate",
                table: "Orders");
        }
    }
}
