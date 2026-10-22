using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NorCommerce.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTripletexCustomerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TripletexCustomerId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TripletexCustomerId",
                table: "AspNetUsers");
        }
    }
}
