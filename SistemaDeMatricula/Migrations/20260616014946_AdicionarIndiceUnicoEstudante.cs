using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaDeMatricula.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarIndiceUnicoEstudante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🎯 1. Índice Único para a tabela de Estudantes
            migrationBuilder.CreateIndex(
                name: "IX_Estudante_Cpf",
                table: "Estudantes",
                column: "Cpf", // Nome exato da coluna no banco (definido no HasColumnName)
                unique: true);

            // 🎯 2. Índice Único para a tabela de Professores
            migrationBuilder.CreateIndex(
                name: "IX_Professor_Cpf",
                table: "Professores",
                column: "Cpf", // Nome exato da coluna no banco (definido no HasColumnName)
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Estudante_Cpf",
                table: "Estudantes");

            migrationBuilder.DropIndex(
                name: "IX_Professor_Cpf",
                table: "Professores");
        }
    }
}