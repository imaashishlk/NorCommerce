using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NorCommerce.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountOrderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TripletexOrderId",
                table: "OrderHeaders",
                newName: "AccountBookOrderId");

            migrationBuilder.RenameColumn(
                name: "TripletexInvoiceId",
                table: "OrderHeaders",
                newName: "AccountBookInvoiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AccountBookOrderId",
                table: "OrderHeaders",
                newName: "TripletexOrderId");

            migrationBuilder.RenameColumn(
                name: "AccountBookInvoiceId",
                table: "OrderHeaders",
                newName: "TripletexInvoiceId");
        }
    }
}
