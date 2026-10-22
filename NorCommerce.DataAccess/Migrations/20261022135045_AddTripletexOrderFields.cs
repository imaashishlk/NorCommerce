using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NorCommerce.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTripletexOrderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InvoiceNumber",
                table: "OrderHeaders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TripletexInvoiceId",
                table: "OrderHeaders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TripletexOrderId",
                table: "OrderHeaders",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "TripletexInvoiceId",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "TripletexOrderId",
                table: "OrderHeaders");
        }
    }
}
