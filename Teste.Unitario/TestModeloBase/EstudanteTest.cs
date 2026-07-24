using Bogus;
using Bogus.Extensions.Brazil;
using FluentAssertions; // Recomendo muito usar para deixar o código legível
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Uteis;
using SistemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Value_Objetc;

namespace SistemaDeMatricula.Teste.Unit.TestModeloBase;

public class EstudanteTest
{
    public static Faker<Estudante> EstudanteFaker => new Faker<Estudante>("pt_BR")
            .CustomInstantiator(f =>
            {
                var dataDateTime = f.Date.Past(20, DateTime.Now.AddYears(-18));
                var dataNascimentoOnly = DateOnly.FromDateTime(dataDateTime);
                var id = Guid.NewGuid();

                return new Estudante(
                    id,
                    new ObjectNomeCompleto(f.Person.FullName),
                    new ObjectDataNascimento(dataNascimentoOnly),
                    new ObjectCPF(f.Person.Cpf(false)),
                    new ObjectEmail(f.Internet.Email()),
                    new ObjectTelefone(f.Phone.PhoneNumber("119########"))
                );
            });

    [Fact]
    public void Deve_Criar_Estudante_Com_Valores_Validos()
    {
        // Arrange & Act
        var estudante = EstudanteFaker.Generate();
        // Assert
        estudante.NomeCompleto.Should().NotBeNull();
        estudante.DataNascimento.Should().NotBeNull();
        estudante.Cpf.Should().NotBeNull();
        estudante.Email.Should().NotBeNull();
        estudante.Telefone.Should().NotBeNull();
    }

    [Fact]
    public void Nao_Deve_Criar_Estudante_Com_Nome_Vazio()
    {
        // Arrange & Act
        Action act = () => new Estudante(Guid.NewGuid(), new ObjectNomeCompleto(""), new ObjectDataNascimento(DateOnly.FromDateTime(DateTime.Now.AddYears(-20))), new ObjectCPF("12345678900"), new ObjectEmail("zandergustavo@gmail.com"), new ObjectTelefone("11999999999"));
        act.Should().Throw<DomainException>();

        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Nao_Deve_Criar_Estudante_Com_Email_Invalido()
    {
        // Arrange & Act
        Action act = () => new Estudante(Guid.NewGuid(), new ObjectNomeCompleto("Gustavo Zander"), new ObjectDataNascimento(DateOnly.FromDateTime(DateTime.Now.AddYears(-20))), new ObjectCPF("12345678900"), new ObjectEmail("email_invalido"), new ObjectTelefone("11999999999"));
        act.Should().Throw<DomainException>();
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Nao_Deve_Criar_Estudante_Com_Telefone_Invalido()
    {
        // Arrange & Act
        Action act = () => new Estudante(Guid.NewGuid(), new ObjectNomeCompleto("Gustavo Zander"), new ObjectDataNascimento(DateOnly.FromDateTime(DateTime.Now.AddYears(-20))), new ObjectCPF("12345678900"), new ObjectEmail("zandergustavo@gmail.com"), new ObjectTelefone("11999999999"));
        act.Should().Throw<DomainException>();
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Nao_Deve_Criar_Estudante_Com_CPF_Invalido()
    {
        // Arrange & Act
        Action act = () => new Estudante(Guid.NewGuid(), new ObjectNomeCompleto("Gustavo Zander"), new ObjectDataNascimento(DateOnly.FromDateTime(DateTime.Now.AddYears(-20))), new ObjectCPF("12345678900"), new ObjectEmail("zandergustavo@gmail.com"), new ObjectTelefone("11999999999"));
        act.Should().Throw<DomainException>();
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Nao_Deve_Criar_Estudante_Com_Data_Nascimento_Invalida()
    {
        // Arrange & Act
        Action act = () => new Estudante(Guid.NewGuid(), new ObjectNomeCompleto("Gustavo Zander"), new ObjectDataNascimento(DateOnly.FromDateTime(DateTime.Now.AddYears(-5))), new ObjectCPF("12345678900"), new ObjectEmail("zandergustavo@gmail.com"), new ObjectTelefone("11999999999"));
        act.Should().Throw<DomainException>();
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Deve_Atualizar_Dados_Do_Estudante()
    {
        // Arrange
        var estudante = EstudanteFaker.Generate();
        var novoNome = new ObjectNomeCompleto("Novo Nome Completo");
        var novaDataNascimento = new ObjectDataNascimento(DateOnly.FromDateTime(DateTime.Now.AddYears(-25)));
        var novoEmail = new ObjectEmail("novoemail@gmail.com");
        var novoTelefone = new ObjectTelefone("11988888888");

        // Act
        estudante.AtualizarDados(novoNome, novoEmail, novaDataNascimento, novoTelefone);

        // Assert
        estudante.NomeCompleto.Should().Be(novoNome);
        estudante.DataNascimento.Should().Be(novaDataNascimento);
        estudante.Email.Should().Be(novoEmail);
        estudante.Telefone.Should().Be(novoTelefone);
    }

    [Fact]
    public void Deve_Adicionar_Matricula_Ao_Estudante()
    {
        // Arrange
        var estudante = EstudanteFaker.Generate();
        var curso = new Disciplina("Disciplina de Teste", 20);
        var matricula = new Matricula(Guid.NewGuid(), Guid.NewGuid());
        // Act
        estudante.AdicionarMatricula(matricula);
        // Assert
        estudante.Matriculas.Should().Contain(matricula);
    }

    [Fact]
    public void Deve_Remover_Matricula_Do_Estudante()
    {
        // Arrange
        var estudante = EstudanteFaker.Generate();
        var curso = new Disciplina("Disciplina de Teste", 20);
        var matricula = new Matricula(Guid.NewGuid(), Guid.NewGuid());
        estudante.AdicionarMatricula(matricula);
        // Act
        estudante.RemoverMatricula(matricula);
        // Assert
        estudante.Matriculas.Should().NotContain(matricula);
    }

    [Fact]
    public void Nao_Deve_Adicionar_Matricula_Duplicada()
    {
        // Arrange
        var estudante = EstudanteFaker.Generate();
        var curso = new Disciplina("Disciplina de Teste", 20);
        var matricula = new Matricula(Guid.NewGuid(), Guid.NewGuid());
        estudante.AdicionarMatricula(matricula);
        // Act
        Action act = () => estudante.AdicionarMatricula(matricula);
        // Assert
        act.Should().Throw<DomainException>();
    }
}