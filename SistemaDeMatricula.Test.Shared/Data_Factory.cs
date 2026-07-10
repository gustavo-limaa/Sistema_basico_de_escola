using Bogus;
using Bogus.Extensions.Brazil;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Uteis;
using SistemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Value_Objetc;

namespace SistemaDeMatricula.Test.Shared;

public class Data_Factory
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
    }).RuleFor(e => e.UsuarioId, f => Guid.NewGuid().ToString());

    public static Faker<Disciplina> DisciplinaFaker => new Faker<Disciplina>()
        .CustomInstantiator(f =>
        {
            var materias = new[] { "Matemática", "Cálculo", "Algoritmos", "Banco de Dados", "História" };
            var nomeSorteado = f.PickRandom(materias) + " " + f.Random.Replace("##");

            return new Disciplina(
                nomeSorteado,
                f.Random.Int(1, 200)
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
    .RuleFor(p => p.Ativo, true)
    .RuleFor(p => p.UsuarioId, f => Guid.NewGuid().ToString());

    public static Faker<Turma> TurmaFaker(Guid? professorId = null, Guid? disciplinaId = null, int? capacidadeForçada = null)
        => new Faker<Turma>("pt_BR")
        .CustomInstantiator(f =>
        {
            var profId = professorId ?? Guid.NewGuid();
            var discId = disciplinaId ?? Guid.NewGuid();
            var capacidade = capacidadeForçada ?? f.Random.Int(10, 100);

            var codigo = new CodigoTurma(
                sigla: f.Random.AlphaNumeric(3).ToUpper(),
                ano: f.Date.Soon().Year,
                semestre: f.Random.Int(1, 2),
                numero: f.Random.Int(1, 999)
            );

            return new Turma(codigo, profId, discId, capacidade);
        });

    public static List<Turma> GerarListaDeTurmas(int quantidade = 50)
    {
        return TurmaFaker().Generate(quantidade);
    }

    public static Faker<Matricula> MatriculaFaker => new Faker<Matricula>("pt_BR")
        .CustomInstantiator(f =>
        {
            // Cria uma matrícula com IDs aleatórios em memória
            return new Matricula(Guid.NewGuid(), Guid.NewGuid());
        });
}
}