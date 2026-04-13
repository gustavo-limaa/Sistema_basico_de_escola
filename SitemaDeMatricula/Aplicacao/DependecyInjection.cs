using SitemaDeMatricula.Aplicacao.Usecases;
using SitemaDeMatricula.Aplicacao.Usecases.Estudante;
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
            services.AddScoped<UsesCasesDeletarEstudante>(); // Esse que você acabou de fazer!

            services.AddScoped<IRepositorioEstudante, RepositorioEstudante>();
            return services;
        }
    }
}