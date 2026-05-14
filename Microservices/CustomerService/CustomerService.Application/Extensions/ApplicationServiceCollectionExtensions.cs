using CustomerService.Application.Services;
using CustomerService.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Shared.Services;

namespace CustomerService.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Servicio CRUD genérico — Shared
        services.AddScoped<BaseService<Customer>>();

        // AppService del dominio Customer
        services.AddScoped<CustomerAppService>();

        return services;
    }
}
