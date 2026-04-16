using SitemaDeMatricula.Aplicacao.Dtos.Professor;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.Domain.Uteis;
using SitemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Value_Objetc;

namespace SitemaDeMatricula.Domain.Mapper
{
    public static class ProfessorMapper
    {
        public static ProfessorDtoResponse ToProfessorDtoResponse(this Professor professor)
        {
            return new ProfessorDtoResponse(
                professor.ProfessorId,
                professor.NomeCompleto.ToString(),
                professor.Cpf.ToString(),
                professor.DataNascimento.Valor,
                professor.Email.ToString(),
                professor.Telefone.ToString(),
                professor.Salario.ToString(),
                professor.Categoria.ToString()
            );
        }

        public static Professor ToProfessor(this ProfessorDtoCreate professorDtoCreate)
        {
            return new Professor(
                new ObjectNomeCompleto(professorDtoCreate.NomeCompleto),
                new ObjectCPF(professorDtoCreate.Cpf),
                new ObjectEmail(professorDtoCreate.Email),
                new ValorMonetario(professorDtoCreate.Salario),
                Enum.Parse<CategoriaProfessor>(professorDtoCreate.Categoria),
                new ObjectDataNascimento(professorDtoCreate.DataNascimento)
            );
        }

        public static void ToAtualizarProfessor(this Professor professor, ProfessorDtoUpdate professorDtoUpdate)
        {
            professor.AtualizarDados(
                new ObjectNomeCompleto(professorDtoUpdate.NomeCompleto),
                new ObjectEmail(professorDtoUpdate.Email),
                new ValorMonetario(professorDtoUpdate.Salario),
                Enum.Parse<CategoriaProfessor>(professorDtoUpdate.Categoria),
                new ObjectDataNascimento(professorDtoUpdate.DataNascimento),
                new ObjectTelefone(professorDtoUpdate.Telefone)
            );
        }
    }
}