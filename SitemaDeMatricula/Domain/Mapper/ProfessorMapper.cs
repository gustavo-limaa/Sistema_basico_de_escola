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
                professor.NomeCompleto.Valor, // Se o Nome for um VO record, use .Valor aqui também!
                professor.Cpf.Valor,               // <--- AQUI! Troque .ToString() por .Valor
                professor.DataNascimento.Valor,
                professor.Email.Valor,             // <--- Use .Valor aqui também
                professor.Telefone.Valor,          // <--- E aqui
                professor.Salario.Valor,
                professor.Categoria.ToString()     // Aqui o .ToString() funciona porque é um Enum
            );
        }

        public static Professor ToProfessor(this ProfessorDtoCreate dto)
        {
            return new Professor(
                new ObjectNomeCompleto(dto.NomeCompleto),
                new ObjectCPF(dto.Cpf),
                new ObjectEmail(dto.Email),
                new ValorMonetario(dto.Salario), // Aqui o decimal entra liso!
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