using SitemaDeMatricula.Aplicacao.Usecases;
using SitemaDeMatricula.Aplicacao.Usecases.Estudante;
using SitemaDeMatricula.Aplicacao.Usecases.Professor;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Infraestrutura.Repositorios;

namespace SitemaDeMatricula.Aplicacao
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Registra todos os Use Cases
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

            services.AddScoped<IRepositorioEstudante, RepositorioEstudante>();
            services.AddScoped<IRepositorioProfessor, RepositorioProfessor>();
            return services;
        }
    }
}