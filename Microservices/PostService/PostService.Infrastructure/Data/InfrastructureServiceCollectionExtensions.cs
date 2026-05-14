using PostService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts;
using Shared.Data;

namespace PostService.Infrastructure.Data;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext — PostService tiene su propia base de datos (solo Post + Category)
        services.AddDbContext<PostDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("PostServiceDb")));

        // Repositorios genéricos — Shared
        services.AddScoped<IBaseRepository<Post>, BaseRepository<Post>>(sp =>
        {
            var context = sp.GetRequiredService<PostDbContext>();
            return new BaseRepository<Post>(context);
        });

        services.AddScoped<IBaseRepository<Category>, BaseRepository<Category>>(sp =>
        {
            var context = sp.GetRequiredService<PostDbContext>();
            return new BaseRepository<Category>(context);
        });

        return services;
    }
}
