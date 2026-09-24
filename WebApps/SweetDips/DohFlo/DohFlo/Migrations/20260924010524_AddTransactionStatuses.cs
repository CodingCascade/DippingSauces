using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DohFlo.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReconciledDate",
                table: "Transactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(
                "UPDATE [Transactions] SET [Status] = 0 WHERE [IsPending] = 1");

            migrationBuilder.DropColumn(
                name: "IsPending",
                table: "Transactions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPending",
                table: "Transactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                "UPDATE [Transactions] SET [IsPending] = 1 WHERE [Status] = 0");

            migrationBuilder.DropColumn(
                name: "ReconciledDate",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Transactions");
        }
    }
}
