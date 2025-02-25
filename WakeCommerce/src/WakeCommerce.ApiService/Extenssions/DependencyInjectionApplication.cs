using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using System.Reflection;
using WakeCommerce.Application.Commands;
using WakeCommerce.Application.Mappings;
using WakeCommerce.Application.Queries.Response;
using WakeCommerce.Application.Queries;
using WakeCommerce.Core.Mediator;
using WakeCommerce.Core.Messages.CommonMessages.Notifications;
using WakeCommerce.Domain.Repositories;
using WakeCommerce.Domain.Repository;
using WakeCommerce.Infrastructure.Data;
using WakeCommerce.Infrastructure.Repository;
using Serilog;
using Serilog.Events;
using WakeCommerce.Application.CommandHandlers;
using WakeCommerce.Application.QueryHandlers;

namespace WakeCommerce.ApiService.Extenssions
{
    public static class DependencyInjectionApplication
    {
        public static void AddDependencies(this IServiceCollection services, WebApplicationBuilder builder)
        {
            AddSerilog(builder);

            // Configuração de serviços
            builder.Services.AddProblemDetails();
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            //builder.Services.AddSerilogLogging();
            builder.Services.AddDatabase(builder.Configuration);
            builder.Services.AddOpenTelemetry(builder.Configuration);
            builder.Services.AddRedisCache(builder.Configuration);
            builder.Services.AddDICors(builder.Configuration);

            builder.Services.AddRepository(builder.Configuration);
            builder.Services.AddHandler(builder.Configuration);

            // Registro do AutoMapper
            builder.Services.AddAutoMapper(typeof(ProdutoProfile).Assembly);

            // Registro do MediatR
            builder.Services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });
        }

        private static void AddSerilog(WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((ctx, lc) => lc
                        .WriteTo.Console(LogEventLevel.Debug)
                        .WriteTo.File($"log-{DateTime.UtcNow.ToString("dd/MM/yyyy")}.txt",
                            LogEventLevel.Warning,
                            rollingInterval: RollingInterval.Day)
                        );
        }

        private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<WakeCommerceDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        }

        private static void AddOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOpenTelemetry()
                    .ConfigureResource(resource => resource.AddService("WakeCommerceAPI"))
                    .WithMetrics(metrics =>
                    {
                        metrics.AddAspNetCoreInstrumentation(); // Monitorar requisições HTTP
                                                 //metrics.AddRuntimeInstrumentation();    // Monitorar .NET Runtime (CPU, GC, Threads)
                        metrics.AddConsoleExporter();           // Exportar métricas para o console
                        metrics.AddPrometheusExporter(options =>
                        {
                            options.ScrapeEndpointPath = "/metrics"; // Definir endpoint
                            options.ScrapeResponseCacheDurationMilliseconds = 5000; // Atualizar a cada 5s
                        });
                    })
                    .WithTracing(tracing =>
                    {
                        tracing.AddAspNetCoreInstrumentation(); // Monitorar requisições HTTP
                        tracing.AddConsoleExporter();           // Exportar traces para o console
                    });

            // Configuração de HealthChecks
            services.AddHealthChecks()
                .AddSqlServer(configuration.GetConnectionString("DefaultConnection"))
                .AddRedis(configuration.GetConnectionString("RedisConnection")!, "Redis");
        }

        private static void AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var connection = configuration.GetConnectionString("RedisConnection");
                return ConnectionMultiplexer.Connect(connection);
            });

            services.AddStackExchangeRedisCache(options =>
            {
                var connection = services.BuildServiceProvider().GetRequiredService<IConnectionMultiplexer>();
                options.ConnectionMultiplexerFactory = () => Task.FromResult(connection);
            });
        }

        private static void AddDICors(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }

        private static void AddRepository(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IProdutoRepository, ProdutoRepository>();
            services.AddScoped<IRedisRepository, RedisRepository>();
        }
        private static void AddHandler(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IRequestHandler<CreateProdutoCommand, ProdutoResponse?>, CreateProdutoCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateProdutoCommand, ProdutoResponse?>, UpdateProdutoCommandHandler>();
            services.AddScoped<IRequestHandler<DeleteProdutoCommand, bool>, DeleteProdutoCommandHandler>();

            services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>();
            services.AddScoped<IMediatorHandler, MediatorHandler>();

            services.AddScoped<IFindProdutoQueryHandler, FindProdutoQueryHandler>();
        }

    }
}
