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
using WakeCommerce.Application.Events;
using WakeCommerce.Application.Events.EventHandlers;
using Serilog.Enrichers.Span;
using Serilog.Sinks.Grafana.Loki;
using OpenTelemetry.Logs;
using WakeCommerce.ServiceDefaults;
using System.Diagnostics;
using WakeCommerce.ApiService.Controllers.Base.ActivitySources;

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
            builder.Services.AddDatabase(builder);
            builder.Services.AddOpenTelemetry(builder);
            builder.Services.AddRedisCache(builder);
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
            //builder.Services.AddSerilog((ctx, lc) => lc
            //            .WriteTo.Console(LogEventLevel.Debug)
            //            .WriteTo.GrafanaLoki(builder.Configuration.GetConnectionString("loki"), new List<LokiLabel>()
            //            {
            //               new LokiLabel()
            //               {
            //                   Key = "service_name",
            //                   Value = "WakeCommercer"
            //               },
            //               new LokiLabel()
            //               {
            //                   Key = "using_database",
            //                   Value = "true"
            //               }
            //            })
            //            .Enrich.WithSpan(new SpanOptions() { IncludeOperationName = true, IncludeTags = true })
            //            );

            //builder.Host.UseSerilog((ctx, lc) => lc
            //            .WriteTo.Console(LogEventLevel.Debug)
            //            .WriteTo.File($"log-{DateTime.UtcNow.ToString("dd/MM/yyyy")}.txt",
            //                LogEventLevel.Warning,
            //                rollingInterval: RollingInterval.Day)
            //            .WriteTo.GrafanaLoki(builder.Configuration.GetConnectionString("loki"), new List<LokiLabel>()
            //            {
            //               new LokiLabel()
            //               {
            //                   Key = "service_name",
            //                   Value = "WakeCommercer"
            //               },
            //               new LokiLabel()
            //               {
            //                   Key = "using_database",
            //                   Value = "true"
            //               }
            //            })
            //            .Enrich.WithSpan(new SpanOptions() { IncludeOperationName = true, IncludeTags = true }));


            //builder.Logging.ClearProviders()
            //    .AddConsole()
            //    .AddDebug()
            //    .AddOpenTelemetry(opt =>
            //    {
            //        opt.AddConsoleExporter()
            //        .SetResourceBuilder(
            //            ResourceBuilder.CreateDefault()
            //            .AddService("Loggin.NET"))
            //            .IncludeScopes = true;
            //    });

            //builder.Services.AddLogging();

            //builder.Host.UseSerilog((context, configuration) =>
            //            configuration.ReadFrom.Configuration(context.Configuration)
            //            .Enrich.WithSpan(new SpanOptions() { IncludeOperationName = true, IncludeTags = true }));
        }

        private static void AddDatabase(this IServiceCollection services, WebApplicationBuilder builder)
        {
            var environment = builder.Environment.EnvironmentName;

            if (environment == "Aspire")
            {
                builder.AddSqlServerDbContext<WakeCommerceDbContext>("sql");
            }
            else
                services.AddDbContext<WakeCommerceDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        }

        private static void AddOpenTelemetry(this IServiceCollection services, WebApplicationBuilder builder)
        {
            var environment = builder.Environment.EnvironmentName;

            services.AddOpenTelemetry()
                    .ConfigureResource(resource => resource.AddService("WakeCommerceApi-2"))
                    .WithMetrics(metrics =>
                    {
                        metrics.AddAspNetCoreInstrumentation(); // Monitorar requisições HTTP
                        metrics.AddRuntimeInstrumentation();    // Monitorar .NET Runtime (CPU, GC, Threads)
                        metrics.AddProcessInstrumentation();
                        metrics.AddSqlClientInstrumentation();
                        //metrics.AddConsoleExporter();           // Exportar métricas para o console

                        metrics.AddMeter(DiagnosticsConfig.Meter.Name);
                        metrics.AddPrometheusExporter(options =>
                        {
                            options.ScrapeEndpointPath = "/metrics"; // Definir endpoint
                            options.ScrapeResponseCacheDurationMilliseconds = 5000; // Atualizar a cada 5s
                        });
                    })
                    .WithTracing(tracing =>
                    {
                        tracing.AddSource("WakeCommerceActivitySource");
                        tracing.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: "WakeCommercer"));
                        tracing.SetSampler(new AlwaysOnSampler());
                        tracing.AddAspNetCoreInstrumentation(); // Monitorar requisições HTTP
                        tracing.AddHttpClientInstrumentation();
                        tracing.AddSqlClientInstrumentation();
                        tracing.AddRedisInstrumentation();
                        tracing.AddConsoleExporter() // Exportar traces para o console
                        .SetSampler(new AlwaysOnSampler());           

                        if(builder.Environment.EnvironmentName != "Aspire")
                        {
                            tracing.AddOtlpExporter(options =>
                             {
                                 options.Endpoint = new Uri(builder.Configuration.GetConnectionString("tempo")); // Endereço do Grafana Tempo
                             }).SetSampler(new AlwaysOnSampler());
                        }
                        else
                        {
                            tracing.AddOtlpExporter();
                        }
                    });

            // Configuração de HealthChecks
            if (environment != "Aspire")
                services.AddHealthChecks()
                        .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
                        .AddRedis(builder.Configuration.GetConnectionString("RedisConnection")!, "Redis");
        }

        private static void AddRedisCache(this IServiceCollection services, WebApplicationBuilder builder)
        {
            var environment = builder.Environment.EnvironmentName;

            if (environment == "Aspire")
            {
                builder.AddRedisDistributedCache("cache");
            }
            else
            {
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var connection = builder.Configuration.GetConnectionString("RedisConnection");
                    return ConnectionMultiplexer.Connect(connection);
                });

                services.AddStackExchangeRedisCache(options =>
                {
                    var connection = services.BuildServiceProvider().GetRequiredService<IConnectionMultiplexer>();
                    options.ConnectionMultiplexerFactory = () => Task.FromResult(connection);
                });
            }

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

            services.AddScoped<INotificationHandler<ProdutoCreateEvent>, ProdutoCreateEventHandler>();

            services.AddScoped<IFindProdutoQueryHandler, FindProdutoQueryHandler>();
        }

    }
}
