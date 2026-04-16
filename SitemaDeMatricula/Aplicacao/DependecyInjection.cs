using SitemaDeMatricula.Aplicacao.Usecases.Disciplinas;
using SitemaDeMatricula.Aplicacao.Usecases.Estudante;
using SitemaDeMatricula.Aplicacao.Usecases.Professor;
using SitemaDeMatricula.Aplicacao.Usecases.Turma;
using SitemaDeMatricula.Aplicaçao.Usecases.Turmas;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Infraestrutura.Repositorios;

namespace SitemaDeMatricula.Aplicacao
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Use Cases
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

            // Repositório
            services.AddScoped<IRepositorioEstudante, RepositorioEstudante>();
            services.AddScoped<IRepositorioProfessor, RepositorioProfessor>();
            services.AddScoped<IDisciplinaRepositorio, DisciplinaRepositorio>();
            services.AddScoped<IRepositorioTurma, RepositorioTurma>();
            // Repositório

            return services;
        }
    }
}