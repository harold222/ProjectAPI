using API.Middleware;
using Application.Services;
using Domain;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

var currentEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

var configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{currentEnvironment}.json", optional: true)
                            .Build();

Log.Logger = new LoggerConfiguration()
                    .ReadFrom
                    .Configuration(configuration)
                    .CreateLogger();

try
{
    Log.Warning("Host starting...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services
        .AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = false;
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "TestAPI", Version = "v1" });
    });

    builder.Services.AddDbContext<JujuTestContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString(currentEnvironment)));

    builder.Services.AddScoped<IBaseRepository<Category>, BaseRepository<Category>>();
    builder.Services.AddScoped<BaseService<Category>, BaseService<Category>>();
    builder.Services.AddScoped<IBaseRepository<Customer>, BaseRepository<Customer>>();
    builder.Services.AddScoped<BaseService<Customer>, BaseService<Customer>>();
    builder.Services.AddScoped<IBaseRepository<Post>, BaseRepository<Post>>();
    builder.Services.AddScoped<BaseService<Post>, BaseService<Post>>();

    builder.Services.AddScoped<CategoryAppService>();
    builder.Services.AddScoped<CustomerAppService>();
    builder.Services.AddScoped<PostAppService>();

    var app = builder.Build();

    app.UseExceptionHandling();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        string swaggerJsonBasePath = string.IsNullOrWhiteSpace(c.RoutePrefix) ? "." : "..";
        c.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/v1/swagger.json", "TestAPI v1");
    });

    app.UseCors(options => options
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}