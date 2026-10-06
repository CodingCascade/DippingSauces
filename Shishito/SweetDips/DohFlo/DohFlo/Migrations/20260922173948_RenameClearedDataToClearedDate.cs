using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DohFlo.Migrations
{
    /// <inheritdoc />
    public partial class RenameClearedDataToClearedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClearedData",
                table: "Transactions",
                newName: "ClearedDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClearedDate",
                table: "Transactions",
                newName: "ClearedData");
        }
    }
}
