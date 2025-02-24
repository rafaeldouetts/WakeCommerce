using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using WakeCommerce.ApiService.Middleware;
using WakeCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using MediatR;
using WakeCommerce.Application.Commands;
using WakeCommerce.Application.CommandHandlers;
using WakeCommerce.Core.Mediator;
using WakeCommerce.Core.Messages.CommonMessages.Notifications;
using WakeCommerce.Domain.Repositories;
using WakeCommerce.Infrastructure.Repository;
using WakeCommerce.Application.Queries;
using WakeCommerce.Application.QueryHandlers;
using WakeCommerce.Application.Queries.Response;
using WakeCommerce.Domain.Repository;
using StackExchange.Redis;
using WakeCommerce.Application.EventHandlers;

var builder = WebApplication.CreateBuilder(args);

// Definindo o environment a partir de uma vari�vel de ambiente ou par�metro customizado
var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"Ambiente:{environment}");

// Configurando o appsettings espec�fico com base no environment
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Config Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

//Config Serilog 
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console(LogEventLevel.Debug)
    .WriteTo.File($"log-{DateTime.UtcNow.ToString("dd/MM/yyyy")}.txt",
        LogEventLevel.Warning,
        rollingInterval: RollingInterval.Day));

// Configura��o do banco de dados (SQL Server)
builder.Services.AddDbContext<WakeCommerceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Configurar OpenTelemetry com logs, m�tricas e tracing
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("WakeCommerceAPI"))
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation(); // Monitorar requisi��es HTTP
        //metrics.AddRuntimeInstrumentation();    // Monitorar .NET Runtime (CPU, GC, Threads)
        metrics.AddConsoleExporter();           // Exportar m�tricas para o console
        metrics.AddPrometheusExporter(); // Exportar m�tricas para o Prometheus
    })
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation(); // Monitorar requisi��es HTTP
        tracing.AddConsoleExporter();           // Exportar traces para o console
    });

// Configurar a conex�o com o Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("RedisConnection");
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IRedisRepository, RedisRepository>();

builder.Services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>();
builder.Services.AddScoped<IMediatorHandler, MediatorHandler>();
builder.Services.AddScoped<IRequestHandler<CreateProdutoCommand, ProdutoResponse?>, CreateProdutoCommandHandler>();
builder.Services.AddScoped<IRequestHandler<UpdateProdutoCommand, ProdutoResponse?>, UpdateProdutoCommandHandler>();
builder.Services.AddScoped<IRequestHandler<DeleteProdutoCommand, bool>, DeleteProdutoCommandHandler>();
builder.Services.AddScoped<IFindProdutoQueryHandler, FindProdutoQueryHandler> ();


builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(ProdutoCreateEventHandler).Assembly));


// Configurar Mediator
builder.Services.AddMediatR(options =>
{
    options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

});

var app = builder.Build();

// Aplica as migra��es pendentes
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WakeCommerceDbContext>();

    // Aplica as migra��es e, em seguida, faz a carga de produtos
    dbContext.Database.Migrate();  // Aplica migra��es pendentes
    dbContext.UploadProdutos();  // Faz a carga de produtos
}

// Add Cors
app.UseCors("AllowSpecificOrigin");

// Add Scalar
app.MapScalarApiReference(opt =>
{
    opt.Title = "Scalar - WakeCommerce";
    opt.Theme = ScalarTheme.Mars;
    opt.DefaultHttpClient = new(ScalarTarget.Http, ScalarClient.Http11);

});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Add Error Middleware Handdling
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();

public partial class Program { }