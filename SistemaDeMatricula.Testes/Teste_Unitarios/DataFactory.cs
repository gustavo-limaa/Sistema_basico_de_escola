// SistemaDeMatricula.Testes\Teste_Unitarios\DataFactory.cs
using Bogus;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.Domain.Uteis;
using SitemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Value_Objetc;
using Xunit.Sdk;
using System.Globalization;

namespace SistemaDeMatricula.Testes.Teste_Unitarios;

public static class DataFactory
{
    public static Faker<Estudante> EstudanteFaker =>
    new Faker<Estudante>("pt_BR")
        .CustomInstantiator(f =>
        {
            var dataDateTime = f.Date.Past(20, DateTime.Now.AddYears(-18));

            var dataNascimentoOnly = DateOnly.FromDateTime(dataDateTime);
            var id = Guid.NewGuid();
            return new Estudante(
                id,
                new ObjectNomeCompleto(f.Person.FullName),
                new ObjectDataNascimento(dataNascimentoOnly),
                new ObjectCPF(f.Random.Replace("###########")),
                new ObjectEmail(f.Internet.Email()),
                new ObjectTelefone(f.Phone.PhoneNumber("119########"))
            );
        });

    public static Faker<Turma> TurmaFaker =>
     new Faker<Turma>("pt_BR")
         .CustomInstantiator(f =>
         {
             var disciplinaId = Guid.NewGuid();

             return new Turma(
                 f.Random.AlphaNumeric(5).ToUpper(),
                 Guid.NewGuid(),
                 disciplinaId
             );
         });

    public static Matricula GerarMatricula(Guid? estudanteId = null, Guid? turmaId = null)
    {
        return new Matricula(
            estudanteId ?? Guid.NewGuid(),
            turmaId ?? Guid.NewGuid()
        );
    }

    public static Faker<Disciplina> DisciplinaFaker => new Faker<Disciplina>()
        .CustomInstantiator(f =>
        {
            return new Disciplina(
                f.Person.FullName,
                f.Random.Int(1, 30)
            );
        });

    public static Faker<Professor> ProfessorFaker => new Faker<Professor>("pt_BR")
    .CustomInstantiator(f =>
    {
        var dataNascimentoOnly = DateOnly.FromDateTime(f.Date.Past(40, DateTime.Now.AddYears(-25)));
        var salariofaker = ValorMonetario.Criar(f.Random.Decimal(3000, 15000)).Resultado;

        return new Professor(
            new ObjectNomeCompleto(f.Person.FullName),
            new ObjectCPF(f.Random.Replace("###########")),
            new ObjectEmail(f.Internet.Email()),
            salariofaker!,
            f.PickRandom<CategoriaProfessor>(),
            new ObjectDataNascimento(dataNascimentoOnly),
            new ObjectTelefone(f.Phone.PhoneNumber("119########"))
        );
    });
};