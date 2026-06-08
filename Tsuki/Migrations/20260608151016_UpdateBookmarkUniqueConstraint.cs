using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tsuki.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookmarkUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookmarks_UserId_ChapterId",
                table: "Bookmarks");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_UserId_NovelId",
                table: "Bookmarks",
                columns: new[] { "UserId", "NovelId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookmarks_UserId_NovelId",
                table: "Bookmarks");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_UserId_ChapterId",
                table: "Bookmarks",
                columns: new[] { "UserId", "ChapterId" },
                unique: true);
        }
    }
}
