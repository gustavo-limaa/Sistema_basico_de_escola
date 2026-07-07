using SistemaDeMatricula.Aplicacao.Usecases.Disciplinas;
using SistemaDeMatricula.Aplicacao.Usecases.Estudante;
using SistemaDeMatricula.Aplicacao.Usecases.Matriculas;
using SistemaDeMatricula.Aplicacao.Usecases.Notas;
using SistemaDeMatricula.Aplicacao.Usecases.Professor;
using SistemaDeMatricula.Aplicacao.Usecases.Turmas;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Infraestrutura.Data;
using SistemaDeMatricula.Infraestrutura.Repositorios;
using SistemaDeMatricula.Services;

namespace SistemaDeMatricula.Infraestrutura
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            #region use cases

            services.AddScoped<RestaurarTurmaUseCase>();
            services.AddScoped<ObterPorIdUsecaseDisciplina>();
            services.AddScoped<ObterTodasDisciplinaUseCase>();
            services.AddScoped<CriarUsecaseDisciplina>();
            services.AddScoped<AtualizarUseCaseDisciplina>();
            services.AddScoped<RemoverUseCaseDisciplina>();
            services.AddScoped<UsesCasesCriarEstudante>();
            services.AddScoped<UsesCasesPegarPorIdEstudante>();
            services.AddScoped<UsesCasesListarTodosEstudante>();
            services.AddScoped<UsesCasesAtualizarEstudante>();
            services.AddScoped<UsesCasesDeletarEstudante>();
            services.AddScoped<ProfessorCriarUsecases>();
            services.AddScoped<ProfessorObterTodosUsecases>();
            services.AddScoped<ProfessorObterPorIdUsecases>();
            services.AddScoped<ProfessorObterPorCpfUsecases>();
            services.AddScoped<ProfessorAtualizarUsecase>();
            services.AddScoped<ProfessorRemoverUsecase>();
            services.AddScoped<CriarTurmaUseCase>();
            services.AddScoped<ListarTurmaUsecase>();
            services.AddScoped<ObterPorIdTurma>();
            services.AddScoped<ObterPorCodigoTurma>();
            services.AddScoped<AtualizarTurmaUseCase>();
            services.AddScoped<RemoverTurmaUseCase>();
            services.AddScoped<RestaurarUseCaseDisciplina>();
            services.AddScoped<ProfessorRestaurarUseCase>();
            services.AddScoped<DesativarMatriculaUsecase>();
            services.AddScoped<MatricularEstudanteUsecase>();
            services.AddScoped<ListarTodasMatriculasUsecase>();
            services.AddScoped<ObterMatriculaPorIdUsecase>();
            services.AddScoped<TransferirEstudanteUsecase>();
            services.AddScoped<ListarTodasAsNotasUsecase>();
            services.AddScoped<ObterNotaPorIdUseCases>();
            services.AddScoped<AdicionarNotasMatriculaUseCase>();
            services.AddScoped<AtualizarNotaUsecase>();

            #endregion use cases

            #region repositories

            services.AddScoped<IRabbitMqProducer, RabbitMqProducer>();
            services.AddScoped<IRepositorioEstudante, RepositorioEstudante>();
            services.AddScoped<IRepositorioProfessor, RepositorioProfessor>();
            services.AddScoped<IDisciplinaRepositorio, DisciplinaRepositorio>();
            services.AddScoped<IRepositorioTurma, RepositorioTurma>();
            services.AddScoped<IRepositorioMatricula, RepositorioMatricula>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRepositorioNotas, RepositorioNotas>();

            #endregion repositories

            #region services

            services.AddScoped<IUsuarioLogadoService, UsuarioLogadoService>();

            #endregion services

            return services;
        }
    }
}