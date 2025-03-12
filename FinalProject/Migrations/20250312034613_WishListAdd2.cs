using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.Migrations
{
    /// <inheritdoc />
    public partial class WishListAdd2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WishlistItems_AspNetUsers_AppUserId",
                table: "WishlistItems");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "WishlistItems");

            migrationBuilder.RenameColumn(
                name: "AppUserId",
                table: "WishlistItems",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_WishlistItems_AppUserId",
                table: "WishlistItems",
                newName: "IX_WishlistItems_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WishlistItems_AspNetUsers_UserId",
                table: "WishlistItems",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WishlistItems_AspNetUsers_UserId",
                table: "WishlistItems");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "WishlistItems",
                newName: "AppUserId");

            migrationBuilder.RenameIndex(
                name: "IX_WishlistItems_UserId",
                table: "WishlistItems",
                newName: "IX_WishlistItems_AppUserId");

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "WishlistItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_WishlistItems_AspNetUsers_AppUserId",
                table: "WishlistItems",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
