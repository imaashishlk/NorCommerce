using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NorCommerce.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceEmailSent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "InvoiceEmailSent",
                table: "OrderHeaders",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceEmailSent",
                table: "OrderHeaders");
        }
    }
}
