using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string environmentName
    )
    {
        // DbContext — configuración de acceso a datos pertenece a Infrastructure
        services.AddDbContext<JujuTestContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(environmentName)));

        // Repositorios — implementaciones de IBaseRepository<T> (contrato definido en Domain)
        services.AddScoped<IBaseRepository<Category>, BaseRepository<Category>>();
        services.AddScoped<IBaseRepository<Customer>, BaseRepository<Customer>>();

        return services;
    }
}
