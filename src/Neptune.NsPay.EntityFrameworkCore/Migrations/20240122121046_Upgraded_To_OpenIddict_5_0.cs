﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neptune.NsPay.Migrations
{
    /// <inheritdoc />
    public partial class Upgraded_To_OpenIddict_5_0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "OpenIddictApplications",
                newName: "ClientType");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationType",
                table: "OpenIddictApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JsonWebKeySet",
                table: "OpenIddictApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Settings",
                table: "OpenIddictApplications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationType",
                table: "OpenIddictApplications");

            migrationBuilder.DropColumn(
                name: "JsonWebKeySet",
                table: "OpenIddictApplications");

            migrationBuilder.DropColumn(
                name: "Settings",
                table: "OpenIddictApplications");

            migrationBuilder.RenameColumn(
                name: "ClientType",
                table: "OpenIddictApplications",
                newName: "Type");
        }
    }
}
