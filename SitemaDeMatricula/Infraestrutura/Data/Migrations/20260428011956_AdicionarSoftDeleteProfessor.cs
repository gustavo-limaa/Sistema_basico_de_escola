using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SitemaDeMatricula.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSoftDeleteProfessor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estudantes_Turmas_TurmaId",
                table: "Estudantes");

            migrationBuilder.DropForeignKey(
                name: "FK_Turmas_Disciplinas_DisciplinaId",
                table: "Turmas");

            migrationBuilder.DropForeignKey(
                name: "FK_Turmas_Professores_ProfessorId",
                table: "Turmas");

            migrationBuilder.DropIndex(
                name: "IX_Estudantes_TurmaId",
                table: "Estudantes");

            migrationBuilder.DropColumn(
                name: "TurmaId",
                table: "Estudantes");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Turmas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DisciplinaId1",
                table: "Turmas",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Professores",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataNascimento_Valor",
                table: "Professores",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Matriculas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Disciplinas",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Disciplinas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Turmas_DisciplinaId1",
                table: "Turmas",
                column: "DisciplinaId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Turmas_Disciplinas_DisciplinaId",
                table: "Turmas",
                column: "DisciplinaId",
                principalTable: "Disciplinas",
                principalColumn: "DisciplinaId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Turmas_Disciplinas_DisciplinaId1",
                table: "Turmas",
                column: "DisciplinaId1",
                principalTable: "Disciplinas",
                principalColumn: "DisciplinaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Turmas_Professores_ProfessorId",
                table: "Turmas",
                column: "ProfessorId",
                principalTable: "Professores",
                principalColumn: "ProfessorId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turmas_Disciplinas_DisciplinaId",
                table: "Turmas");

            migrationBuilder.DropForeignKey(
                name: "FK_Turmas_Disciplinas_DisciplinaId1",
                table: "Turmas");

            migrationBuilder.DropForeignKey(
                name: "FK_Turmas_Professores_ProfessorId",
                table: "Turmas");

            migrationBuilder.DropIndex(
                name: "IX_Turmas_DisciplinaId1",
                table: "Turmas");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Turmas");

            migrationBuilder.DropColumn(
                name: "DisciplinaId1",
                table: "Turmas");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Professores");

            migrationBuilder.DropColumn(
                name: "DataNascimento_Valor",
                table: "Professores");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Matriculas");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Disciplinas");

            migrationBuilder.AddColumn<Guid>(
                name: "TurmaId",
                table: "Estudantes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Disciplinas",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Estudantes_TurmaId",
                table: "Estudantes",
                column: "TurmaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Estudantes_Turmas_TurmaId",
                table: "Estudantes",
                column: "TurmaId",
                principalTable: "Turmas",
                principalColumn: "TurmaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Turmas_Disciplinas_DisciplinaId",
                table: "Turmas",
                column: "DisciplinaId",
                principalTable: "Disciplinas",
                principalColumn: "DisciplinaId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Turmas_Professores_ProfessorId",
                table: "Turmas",
                column: "ProfessorId",
                principalTable: "Professores",
                principalColumn: "ProfessorId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
