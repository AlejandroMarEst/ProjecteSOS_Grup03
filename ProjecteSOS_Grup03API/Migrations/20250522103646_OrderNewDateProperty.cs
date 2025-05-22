using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjecteSOS_Grup03API.Migrations
{
    /// <inheritdoc />
    public partial class OrderNewDateProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "OrderDate",
                table: "Orders",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderDate",
                table: "Orders");
        }
    }
}
