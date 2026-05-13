//SistemaDeMatricula.Testes\Teste_Unitarios\DataFactory.cs
using Bogus;
using Bogus.Extensions.Brazil;
using SitemaDeMatricula.Domain.Value_Objetc;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Domain.Uteis;
using SistemaDeMatricula.Domain.Value_Object;

namespace SistemaDeMatricula.Testes.Teste_Unitarios;

public static class DataFactory
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
            var materias = new[] { "Matemática", "Cálculo", "Algoritmos", "Banco de Dados", "História" };
            var nomeSorteado = f.PickRandom(materias) + " " + f.Random.Replace("##");

            return new Disciplina(
                nomeSorteado,
                f.Random.Int(1, 200) // Disciplinas geralmente têm mais horas que 1-30
            );
        });

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

    public static Faker<EstudanteDtoUpdate> EstudanteDtoUpdateFaker => new Faker<EstudanteDtoUpdate>("pt_BR")
        .CustomInstantiator(f =>
        {
            var dataDateTime = f.Date.Past(20, DateTime.Now.AddYears(-18));
            var dataNascimentoOnly = DateOnly.FromDateTime(dataDateTime);

            return new EstudanteDtoUpdate(
                f.Person.FullName,                 // 1. NomeCompleto
                f.Internet.Email(),                // 2. Email
                dataNascimentoOnly,                // 3. DataNascimento
                f.Phone.PhoneNumber("119########") // 4. Telefone
            );
        });

    public static Faker<Turma> TurmaFaker(Guid? professorId = null, Guid? disciplinaId = null)
    => new Faker<Turma>("pt_BR")
    .CustomInstantiator(f =>
    {
        // Se eu passar um ID, ele usa. Se não, ele gera um novo (útil para testes unitários)
        var profId = professorId ?? Guid.NewGuid();
        var discId = disciplinaId ?? Guid.NewGuid();

        var codigo = new CodigoTurma(
            sigla: f.Random.AlphaNumeric(3).ToUpper(),
            ano: f.Date.Soon().Year,
            semestre: f.Random.Int(1, 2),
            numero: f.Random.Int(1, 999)
        );

        var capacidade = f.Random.Int(0, 5000);
        return new Turma(codigo, profId, discId, capacidade);
    });

    public static List<Turma> GerarListaDeTurmas(int quantidade = 50)
    {
        return TurmaFaker().Generate(quantidade);
    }
}