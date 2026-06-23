//SistemaDeMatricula.Testes\Teste_Unitarios\DataFactory.cs
using Bogus;
using Bogus.Extensions.Brazil;
using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Uteis;
using SistemaDeMatricula.Domain.Value_Object;

using SistemaDeMatricula.Domain.Value_Object;

using SistemaDeMatricula.Infraestrutura.Data;
using SitemaDeMatricula.Domain.Value_Objetc;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    })
    .RuleFor(e => e.UsuarioId, f => Guid.NewGuid().ToString());

    // No DataFactory.cs
    public static async Task<(Estudante estudante, Turma turma, Matricula matricula)> CriarCenarioDeMatriculaValido(
    AppDbContext contexto,
    int capacidade = 50) // <-- Adicione esse parâmetro opcional
    {
        var disciplina = DisciplinaFaker.Generate();
        var professor = ProfessorFaker.Generate();
        var estudante = EstudanteFaker.Generate();
        estudante.ativar();
        professor.ativar();
        disciplina.ativar();

        await contexto.Disciplinas.AddAsync(disciplina);
        await contexto.Professores.AddAsync(professor);
        await contexto.Estudantes.AddAsync(estudante);

        await contexto.SaveChangesAsync();

        // Agora passamos a 'capacidade' que recebemos no argumento
        var turma = TurmaFaker(professor.Id, disciplina.Id, capacidade).Generate();
        turma.ativar();

        if (!turma.Ativo || !estudante.Ativo)
            throw new Exception("Cenário criado com entidades inativas!");

        await contexto.Turmas.AddAsync(turma);

        // IMPORTANTE: Para a turma estar lotada, precisamos matricular esse primeiro estudante
        var matricula = new Matricula(estudante.Id, turma.Id);
        matricula.ativar();
        await contexto.Matriculas.AddAsync(matricula);
        await contexto.SaveChangesAsync();

        return (estudante, turma, matricula);
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
    // Força o estado ativo após a instância ser criada
    .RuleFor(p => p.Ativo, true)
    // 🎯 INJETANDO O ID FAKE DO IDENTITY PARA O PROFESSOR
    .RuleFor(p => p.UsuarioId, f => Guid.NewGuid().ToString());

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

    public static Faker<Turma> TurmaFaker(Guid? professorId = null, Guid? disciplinaId = null, int? capacidadeForçada = null)
        => new Faker<Turma>("pt_BR")
        .CustomInstantiator(f =>
        {
            var profId = professorId ?? Guid.NewGuid();
            var discId = disciplinaId ?? Guid.NewGuid();

            // Se 'capacidadeForçada' tiver valor (vinda do teste), usa ela.
            // Se for null, sorteia o aleatório (mantém compatibilidade com outros testes).
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