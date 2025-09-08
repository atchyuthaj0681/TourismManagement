using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourismManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelWishlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DestinationImages_Destinations_DestinationId",
                table: "DestinationImages");

            migrationBuilder.DropTable(
                name: "Destinations");

            migrationBuilder.RenameColumn(
                name: "DestinationId",
                table: "DestinationImages",
                newName: "TravelWishlistItemId");

            migrationBuilder.RenameIndex(
                name: "IX_DestinationImages_DestinationId",
                table: "DestinationImages",
                newName: "IX_DestinationImages_TravelWishlistItemId");

            migrationBuilder.CreateTable(
                name: "TravelWishlistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    BestTimeToVisit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelWishlistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelWishlistItems_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TravelWishlistItems_UserId",
                table: "TravelWishlistItems",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DestinationImages_TravelWishlistItems_TravelWishlistItemId",
                table: "DestinationImages",
                column: "TravelWishlistItemId",
                principalTable: "TravelWishlistItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DestinationImages_TravelWishlistItems_TravelWishlistItemId",
                table: "DestinationImages");

            migrationBuilder.DropTable(
                name: "TravelWishlistItems");

            migrationBuilder.RenameColumn(
                name: "TravelWishlistItemId",
                table: "DestinationImages",
                newName: "DestinationId");

            migrationBuilder.RenameIndex(
                name: "IX_DestinationImages_TravelWishlistItemId",
                table: "DestinationImages",
                newName: "IX_DestinationImages_DestinationId");

            migrationBuilder.CreateTable(
                name: "Destinations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BestTimeToVisit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destinations", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_DestinationImages_Destinations_DestinationId",
                table: "DestinationImages",
                column: "DestinationId",
                principalTable: "Destinations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
