using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DohFlo.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedUserTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 26, 15, 55, 0, 0, DateTimeKind.Utc), new DateTime(2026, 4, 26, 15, 55, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 24, 23, 56, 17, 473, DateTimeKind.Utc).AddTicks(2863), new DateTime(2026, 3, 24, 23, 56, 17, 473, DateTimeKind.Utc).AddTicks(3164) });
        }
    }
}
