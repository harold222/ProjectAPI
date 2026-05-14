using Microsoft.Extensions.DependencyInjection;
using PostService.Application.Services;
using PostService.Domain.Entities;
using Shared.Services;

namespace PostService.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Application layer services.
    /// ICustomerServiceClient is NOT registered here — it's an interface defined by
    /// Application and implemented by Infrastructure (Dependency Inversion).
    /// The Infrastructure layer registers its HttpClient-based implementation.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Servicios CRUD genéricos — Shared
        services.AddScoped<BaseService<Post>>();
        services.AddScoped<BaseService<Category>>();

        // AppServices del dominio Post
        services.AddScoped<CategoryAppService>();
        services.AddScoped<PostAppService>();

        return services;
    }
}
