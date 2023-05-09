using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_201.Migrations
{
    /// <inheritdoc />
    public partial class AddedSectionLogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SectionLogo",
                table: "Sections",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SectionLogo",
                table: "Sections");
        }
    }
}
