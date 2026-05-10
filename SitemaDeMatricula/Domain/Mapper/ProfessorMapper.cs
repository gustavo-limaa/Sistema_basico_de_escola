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
                professor.NomeCompleto.Valor,
                professor.Cpf.Valor,
                professor.DataNascimento.Valor,
                professor.Email.Valor,
                professor.Telefone.Valor,
                professor.Salario.Valor,
                professor.Categoria.ToString()
            );
        }

        public static Professor ToProfessor(this ProfessorDtoCreate dto)
        {
            return new Professor(
                new ObjectNomeCompleto(dto.NomeCompleto),
                new ObjectCPF(dto.Cpf),
                new ObjectEmail(dto.Email),
                new ValorMonetario(dto.Salario),
                Enum.Parse<CategoriaProfessor>(dto.Categoria),
                new ObjectDataNascimento(dto.DataNascimento),
                new ObjectTelefone(dto.Telefone)
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