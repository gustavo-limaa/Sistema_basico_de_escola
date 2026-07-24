using Bogus;
using Bogus.Extensions.Brazil;
using SistemaDeMatricula.Aplicacao.Dtos.Disciplina;
using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Aplicacao.Dtos.Matricola;
using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Aplicacao.Dtos.Professor;
using SistemaDeMatricula.Aplicacao.Dtos.turma;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Uteis;
using SistemaDeMatricula.Domain.Value_Object;
using SistemaDeMatricula.Infraestrutura.Data;
using SitemaDeMatricula.Domain.Value_Objetc;

namespace SistemaDeMatricula.Test.Shared;

public static class Data_Factory
{
    public static async Task<(Estudante, Turma, Matricula)> CriarCenarioDeMatriculaValido(AppDbContext contexto)
    {
        var estudante = EstudanteFaker.Generate();
        var disciplina = DisciplinaFaker.Generate();
        var professor = ProfessorFaker.Generate();

        contexto.Estudantes.Add(estudante);
        contexto.Disciplinas.Add(disciplina);
        contexto.Professores.Add(professor);
        await contexto.SaveChangesAsync(); // salva as entidades "pai" primeiro

        var turma = TurmaFaker(professor.Id, disciplina.Id, 30).Generate();
        contexto.Turmas.Add(turma);
        await contexto.SaveChangesAsync();

        var matricula = new Matricula(estudante.Id, turma.Id);
        contexto.Matriculas.Add(matricula);
        await contexto.SaveChangesAsync();

        return (estudante, turma, matricula);
    }

    public static Faker<Matricula> MatriculaFaker(Guid estudanteId, Guid turmaId) =>
    new Faker<Matricula>()
        .CustomInstantiator(f =>
        {
            return new Matricula(estudanteId, turmaId);
        });

    public static Faker<Disciplina> DisciplinaFaker => new Faker<Disciplina>()
        .CustomInstantiator(f =>
        {
            var materias = new[] { "Matemática", "Cálculo", "Algoritmos", "Banco de Dados", "História" };
            var nomeSorteado = f.PickRandom(materias) + " " + f.Random.Replace("######");

            return new Disciplina(
                nome: nomeSorteado,
                cargaHoraria: f.Random.Int(30, 120)
            );
        });

    public static Faker<Turma> TurmaFaker(Guid professorId, Guid disciplinaId, int capacidade) =>
    new Faker<Turma>("pt_BR")
        .CustomInstantiator(f =>
        {
            var siglaValida = f.Random.String2(3, "ABCDEFGHIJKLMNOPQRSTUVWXYZ"); // Gera 3 letras
            var anoAtual = DateTime.Now.Year;
            var semestreValido = f.Random.ListItem(new[] { 1, 2 }); // Escolhe 1 ou 2
            var numeroTurma = f.Random.Number(1, 99);

            var codigoTurma = new CodigoTurma(siglaValida, anoAtual, semestreValido, numeroTurma);

            return new Turma(
                codigoTurma,
                professorId,
                disciplinaId,

                capacidade

            );
        });

    public static Faker<Estudante> EstudanteFaker => new Faker<Estudante>("pt_BR")
        .CustomInstantiator(f =>
        {
            var dataDateTime = f.Date.Past(20, DateTime.Now.AddYears(-18));
            var dataNascimentoOnly = DateOnly.FromDateTime(dataDateTime);
            var id = Guid.NewGuid();

            // 1. Cria a instância usando o construtor padrão do domínio
            var estudante = new Estudante(
                id,
                new ObjectNomeCompleto(f.Person.FullName),
                new ObjectDataNascimento(dataNascimentoOnly),
                new ObjectCPF(f.Person.Cpf(false)),
                new ObjectEmail(f.Internet.Email()),
                new ObjectTelefone(f.Phone.PhoneNumber("119########"))
            );

            // 🎯 O PULO DO GATO DO DDD: Chama o método de negócio para injetar um ID de usuário fake!
            estudante.VincularUsuario(f.Random.Guid().ToString());

            return estudante;
        });

    public static Faker<Professor> ProfessorFaker => new Faker<Professor>("pt_BR")
 .CustomInstantiator(f =>
 {
     var dataNascimentoOnly = DateOnly.FromDateTime(f.Date.Past(40, DateTime.Now.AddYears(-25)));

     var professor = new Professor(
         new ObjectNomeCompleto(f.Person.FullName),
         new ObjectCPF(f.Person.Cpf(false)),
         new ObjectEmail(f.Internet.Email()),
         new ValorMonetario(Math.Round(f.Random.Decimal(3000, 15000), 2)),
         f.PickRandom<CategoriaProfessor>(),
         new ObjectDataNascimento(dataNascimentoOnly),
         new ObjectTelefone(f.Phone.PhoneNumber("119########"))
     );

     professor.VincularUsuario(f.Random.Guid().ToString());

     return professor;
 })
 .RuleFor(p => p.Ativo, true);

    public static Faker<EstudanteDtoCreate> EstudanteFakerdto => new Faker<EstudanteDtoCreate>("pt_BR")
    .CustomInstantiator((Func<Faker, EstudanteDtoCreate>)(f =>
    {
        var dto = Data_Factory.EstudanteFaker.Generate();

        return new EstudanteDtoCreate(
            NomeCompleto: (string)dto.NomeCompleto.Valor,
            DataNascimento: (DateOnly)dto.DataNascimento.Valor,
            Cpf: (string)dto.Cpf.Valor,
            Email: (string)dto.Email.Valor,
            Telefone: (string)dto.Telefone.Valor
        );
    }));

    public static Faker<DisciplinaDtoCreate> DisciplinaFakerdto => new Faker<DisciplinaDtoCreate>()
        .CustomInstantiator(f =>
         {
             var dto = Data_Factory.DisciplinaFaker.Generate();
             return new DisciplinaDtoCreate(
                Nome: dto.Nome.Valor,
                CargaHoraria: dto.CargaHoraria.Valor
            );
         });

    public static Faker<ProfessorDtoCreate> ProfessorFakerdto => new Faker<ProfessorDtoCreate>("pt_BR")
    .CustomInstantiator(f =>
    {
        var dto = Data_Factory.ProfessorFaker.Generate();
        return new ProfessorDtoCreate(
         NomeCompleto: dto.NomeCompleto.Valor,
            DataNascimento: dto.DataNascimento.Valor,
            Cpf: dto.Cpf.Valor,
            Email: dto.Email.Valor,
            Telefone: dto.Telefone.Valor,
            Salario: dto.Salario.Valor,
            Categoria: dto.Categoria.ToString()
        );
    });

    public static Faker<TurmaDtoCreate> TurmaFakerdto(Guid professorId, Guid disciplinaId, int capacidadeForçada)
        => new Faker<TurmaDtoCreate>("pt_BR")
        .CustomInstantiator(f =>
        {
            var dto = Data_Factory.TurmaFaker(professorId, disciplinaId, capacidadeForçada).Generate();

            return new TurmaDtoCreate(
                DisciplinaId: dto.DisciplinaId,
                ProfessorId: dto.ProfessorId,
                CapacidadeMaxima: dto.CapacidadeMaxima,
                Sigla: dto.CodigoTurma.Sigla,
                Semestre: dto.CodigoTurma.Semestre,
                AnoLetivo: dto.CodigoTurma.Ano,
                Numero: dto.CodigoTurma.Numero
            );
        });

    public static Faker<MatriculaDtoCreate> MatriculaFakerdto => new Faker<MatriculaDtoCreate>("pt_BR")
        .CustomInstantiator(f =>
        {
            return new MatriculaDtoCreate(
                EstudanteId: Guid.NewGuid(),
                TurmaId: Guid.NewGuid()
            );
        });

    public static Faker<EstudanteDtoUpdate> EstudanteFakerup => new Faker<EstudanteDtoUpdate>("pt_BR")
        .CustomInstantiator(f =>
        {
            var dto = Data_Factory.EstudanteFakerdto.Generate();
            return new EstudanteDtoUpdate(
                NomeCompleto: dto.NomeCompleto,
                Email: dto.Email,
                DataNascimento: dto.DataNascimento,
                Telefone: dto.Telefone
            );
        });

    public static Faker<ProfessorDtoUpdate> ProfessorFakerup => new Faker<ProfessorDtoUpdate>("pt_BR")
        .CustomInstantiator(f =>
        {
            var dto = Data_Factory.ProfessorFakerdto.Generate();
            return new ProfessorDtoUpdate(
                ProfessorId: Guid.NewGuid(),
                NomeCompleto: dto.NomeCompleto,
                Email: dto.Email,
                DataNascimento: dto.DataNascimento,
                Telefone: dto.Telefone,
                Salario: dto.Salario,
                Categoria: dto.Categoria
            );
        });

    public static Faker<DisciplinaDtoUpdate> DisciplinaFakerup => new Faker<DisciplinaDtoUpdate>()
        .CustomInstantiator(f =>
        {
            var dto = Data_Factory.DisciplinaFakerdto.Generate();
            return new DisciplinaDtoUpdate(
                DisciplinaId: Guid.NewGuid(),
                Nome: dto.Nome,
                CargaHoraria: dto.CargaHoraria,
                true
            );
        });

    public static Faker<TurmaDtoUpdate> TurmaFakerup(Guid professorId, Guid disciplinaId, int capacidadeForçada)
        => new Faker<TurmaDtoUpdate>("pt_BR")
        .CustomInstantiator(f =>
        {
            var dto = Data_Factory.TurmaFakerdto(professorId, disciplinaId, capacidadeForçada).Generate();
            return new TurmaDtoUpdate(
                ProfessorId: dto.ProfessorId,
                DisciplinaId: dto.DisciplinaId,
                 novaCapacidade: dto.CapacidadeMaxima
                 , true,
                Sigla: dto.Sigla,
                Semestre: dto.Semestre,
                AnoLetivo: dto.AnoLetivo,
                Numero: dto.Numero
            );
        });

    public static Faker<Nota> NotaFaker => new Faker<Nota>("pt_BR")
        .CustomInstantiator(n =>
        {
            var matriculaId = Guid.NewGuid();
            var importancia = n.PickRandom<TipoImportancia>();
            var categoria = n.PickRandom<CategoriaAvaliacao>();

            var valorNota = Math.Round(n.Random.Double(0, 10), 1);

            var descricao = n.Lorem.Sentence(3);

            var dataEmissao = DateTime.UtcNow;

            return new Nota(
                matriculaId,
                importancia,
                categoria,
                valorNota,
                descricao,
                dataEmissao
            );
        });

    public static Faker<NotaDtoCreate> NotafakerDto = new Faker<NotaDtoCreate>("pt_BR").CustomInstantiator(n =>
    {
        var Dto = Data_Factory.NotaFaker.Generate();
        return new NotaDtoCreate
        (
            Valor: Dto.Valor,
            Descricao: Dto.Descricao,
            Importancia: Dto.Importancia,
            Categoria: Dto.Categoria
        );
    }

    );

    public static Faker<NotaDtoUpdate> Notafakerup = new Faker<NotaDtoUpdate>().CustomInstantiator(n =>
    {
        var dto = Data_Factory.NotaFaker.Generate();

        return new NotaDtoUpdate
        (
            Valor: dto.Valor,
            Descricao: dto.Descricao,
            Importancia: dto.Importancia,
            Categoria: dto.Categoria
        );
    });
}