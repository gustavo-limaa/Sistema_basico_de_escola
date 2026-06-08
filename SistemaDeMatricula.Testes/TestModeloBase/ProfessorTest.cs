using Bogus;
using Bogus.Extensions.Brazil;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Uteis;
using SistemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Value_Objetc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.TestModeloBase
{
    public class ProfessorTest
    {
        public static Faker<Professor> ProfessorFaker => new Faker<Professor>("pt_BR")
     .CustomInstantiator(f =>
     {
         var dataNascimentoOnly = DateOnly.FromDateTime(f.Date.Past(40, DateTime.Now.AddYears(-25)));

         return new Professor(
             new ObjectNomeCompleto(f.Person.FullName),
             new ObjectCPF(f.Person.Cpf(false)),
             new ObjectEmail(f.Internet.Email()),
             new ValorMonetario(Math.Round(f.Random.Decimal(3000, 15000), 2)),
             f.PickRandom<CategoriaProfessor>(),
             new ObjectDataNascimento(dataNascimentoOnly),
             new ObjectTelefone(f.Phone.PhoneNumber("119########"))
         );
     })
     // O PULO DO GATO: Força o estado ativo após a instância ser criada
     .RuleFor(p => p.Ativo, true);

        [Fact]
        public void CriarProfessor_Valido_DeveCriarComSucesso()
        {
            // Arrange
            var professor = ProfessorFaker.Generate();
            // Act & Assert
            Assert.NotNull(professor);
            Assert.False(string.IsNullOrWhiteSpace(professor.NomeCompleto.Valor));
            Assert.False(string.IsNullOrWhiteSpace(professor.Cpf.Valor));
            Assert.False(string.IsNullOrWhiteSpace(professor.Email.Valor));
            Assert.True(professor.Salario.Valor > 0);
            Assert.NotEqual(default, professor.DataNascimento.Valor);
            Assert.False(string.IsNullOrWhiteSpace(professor.Telefone.Valor));
        }

        [Fact]
        public void AtualizarDados_ProfessorAtivo_DeveAtualizarComSucesso()
        {
            // Arrange
            var professor = ProfessorFaker.Generate();
            professor.AtualizarDados(
                new ObjectNomeCompleto("Novo Nome Completo"),
                new ObjectEmail("novoemail@example.com"),
                new ValorMonetario(5000),
                CategoriaProfessor.Titular,
                new ObjectDataNascimento(DateOnly.FromDateTime(DateTime.Now.AddYears(-30))),
                new ObjectTelefone("11987654321")
            );

            // Assert
            Assert.Equal("Novo Nome Completo", professor.NomeCompleto.Valor);
            Assert.Equal("novoemail@example.com", professor.Email.Valor);
            Assert.Equal(5000, professor.Salario.Valor);
            Assert.Equal(CategoriaProfessor.Titular, professor.Categoria);
            Assert.Equal(DateOnly.FromDateTime(DateTime.Now.AddYears(-30)), professor.DataNascimento.Valor);
            Assert.Equal("11987654321", professor.Telefone.Valor);
        }

        [Fact]
        public void AtualizarDados_ProfessorDesativado_DeveLancarDomainException()
        {
            // Arrange
            var professor = ProfessorFaker.Generate();
            professor.Desativar();
            // Act
            Action act = () => professor.AtualizarDados(
                new ObjectNomeCompleto("Novo Nome Completo"),
                new ObjectEmail("novoemail@example.com"),
                new ValorMonetario(5000),
                CategoriaProfessor.Titular,
                new ObjectDataNascimento(DateOnly.FromDateTime(DateTime.Now.AddYears(-30))),
                new ObjectTelefone("11987654321")
            );
            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void Ativar_ProfessorDesativado_DeveAtivarComSucesso()
        {
            // Arrange
            var professor = ProfessorFaker.Generate();
            professor.Desativar();
            // Act
            professor.Ativar();
            // Assert
            Assert.True(professor.Ativo);
        }
    }
}