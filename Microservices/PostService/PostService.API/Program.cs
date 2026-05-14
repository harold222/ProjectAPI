using PostService.API.Middleware;
using PostService.Application.Extensions;
using PostService.Application.Interfaces;
using PostService.Infrastructure.Clients;
using PostService.Infrastructure.Data;
using Serilog;

var currentEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{currentEnvironment}.json", optional: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Warning("PostService starting...");

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
        c.SwaggerDoc("v1", new() { Title = "PostService", Version = "v1" });
    });

    // Composition Root — cada capa registra sus propios servicios
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(configuration);

    // 🔗 INTER-SERVICE HTTP CLIENT — PostService → CustomerService
    // Register ICustomerServiceClient with HttpClient pointing to CustomerService's URL
    builder.Services.AddHttpClient<ICustomerServiceClient, CustomerServiceClient>(client =>
    {
        client.BaseAddress = new Uri(configuration["CustomerService:BaseUrl"]
            ?? "https://localhost:5001");
    });

    var app = builder.Build();

    app.UseExceptionHandling();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        string swaggerJsonBasePath = string.IsNullOrWhiteSpace(c.RoutePrefix) ? "." : "..";
        c.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/v1/swagger.json", "PostService v1");
    });

    app.UseCors(options => options
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

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
