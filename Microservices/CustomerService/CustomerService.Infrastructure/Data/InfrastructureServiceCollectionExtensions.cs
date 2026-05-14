using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts;
using Shared.Data;

namespace CustomerService.Infrastructure.Data;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext — cada microservicio apunta a su propia base de datos
        services.AddDbContext<CustomerDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("CustomerServiceDb")));

        // Repositorio genérico — implementación de Shared
        services.AddScoped<IBaseRepository<Customer>, BaseRepository<Customer>>(sp =>
        {
            var context = sp.GetRequiredService<CustomerDbContext>();
            return new BaseRepository<Customer>(context);
        });

        return services;
    }
}
