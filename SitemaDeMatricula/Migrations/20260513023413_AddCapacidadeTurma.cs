using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SitemaDeMatricula.Migrations
{
    /// <inheritdoc />
    public partial class AddCapacidadeTurma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
            name: "CapacidadeMaxima",
            table: "Turmas",
            type: "int",
            nullable: false,
            defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapacidadeMaxima",
                table: "Turmas");
        }
    }
}