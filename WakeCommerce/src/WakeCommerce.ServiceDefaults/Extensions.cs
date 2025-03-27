using System.Diagnostics.Metrics;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace WakeCommerce.ServiceDefaults
{
    public static class Extensions
    {
        public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            builder.AddDefaultHealthChecks();

            // Uncomment the following to restrict the allowed schemes for service discovery.
            // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
            // {
            //     options.AllowedSchemes = ["https"];
            // });

            return builder;
        }

        public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
                logging.SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                        .AddService("Loggin.NET"));
            });

            //builder.Services.AddOpenTelemetry()
            //    .WithTracing(tracing =>
            //    {
            //        tracing
            //            .AddSource("WakeCommerceActivitySource")
            //            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: "WakeCommerceApi", serviceVersion:"1"))
            //            .AddAspNetCoreInstrumentation()
            //            .AddHttpClientInstrumentation()
            //            .AddSqlClientInstrumentation()
            //            .SetSampler(new AlwaysOnSampler())
            //            .AddRedisInstrumentation()
            //            // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
            //            //.AddGrpcClientInstrumentation()
            //            .AddHttpClientInstrumentation();

            //        if (builder.Environment.EnvironmentName != "Aspire")
            //        {
            //            tracing.AddOtlpExporter(options =>
            //            {
            //                options.Endpoint = new Uri(builder.Configuration.GetConnectionString("tempo")); // Endereço do Grafana Tempo
            //            }).SetSampler(new AlwaysOnSampler());
            //        }
            //        else
            //        {
            //            tracing.AddOtlpExporter();
            //        }
            //    })
            //    .WithMetrics(metrics =>
            //    {
            //        metrics
            //            .AddAspNetCoreInstrumentation()
            //            .AddHttpClientInstrumentation()
            //            .AddProcessInstrumentation()
            //            .AddRuntimeInstrumentation()
            //            .AddSqlClientInstrumentation()
            //            .AddMeter(DiagnosticsConfig.Meter.Name)
            //            .AddPrometheusExporter();
            //    });


            builder.AddOpenTelemetryExporters();

            return builder;
        }

        private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

            if (useOtlpExporter)
            {
                builder.Services.AddOpenTelemetry()
                    .UseOtlpExporter();
            }

            // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
            //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
            //{
            //    builder.Services.AddOpenTelemetry()
            //       .UseAzureMonitor();
            //}

            return builder;
        }

        public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            builder.Services.AddHealthChecks()
                // Add a default liveness check to ensure app is responsive
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

            return builder;
        }

        public static WebApplication MapDefaultEndpoints(this WebApplication app)
        {
            // Adding health checks endpoints to applications in non-development environments has security implications.
            // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
            if (app.Environment.IsDevelopment())
            {
                // All health checks must pass for app to be considered ready to accept traffic after starting
                app.MapHealthChecks("/health");

                // Only health checks tagged with the "live" tag must pass for app to be considered alive
                app.MapHealthChecks("/alive", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("live")
                });
            }

            app.UseOpenTelemetryPrometheusScrapingEndpoint();

            return app;
        }

        public static class DiagnosticsConfig
        {
            public const string ServiceName = "WakeCommerceActivitySource";

            public static Meter Meter = new(ServiceName);
            public static ActivitySource Source = new(ServiceName);

            public static Counter<long> ProdutosCadastrados = Meter.CreateCounter<long>("Produtos.count");
        }
    }
}
