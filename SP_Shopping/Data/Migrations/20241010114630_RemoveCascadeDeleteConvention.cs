using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SP_Shopping.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCascadeDeleteConvention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubmitterId",
                table: "Products",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SubmitterId",
                table: "Products",
                column: "SubmitterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_AspNetUsers_SubmitterId",
                table: "Products",
                column: "SubmitterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_AspNetUsers_SubmitterId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SubmitterId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SubmitterId",
                table: "Products");
        }
    }
}
