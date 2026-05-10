using Application.Services;
using Domain;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // BaseService<T> — orquesta CRUD genérico, depende de IBaseRepository<T> (Domain)
        services.AddScoped<BaseService<Category>, BaseService<Category>>();
        services.AddScoped<BaseService<Customer>, BaseService<Customer>>();
        services.AddScoped<BaseService<Post>, BaseService<Post>>();

        // AppServices — lógica de negocio específica
        services.AddScoped<CategoryAppService>();
        services.AddScoped<CustomerAppService>();
        services.AddScoped<PostAppService>();

        return services;
    }
}
