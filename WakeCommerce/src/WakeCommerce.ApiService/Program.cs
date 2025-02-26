using Scalar.AspNetCore;
using WakeCommerce.ApiService.Middleware;
using WakeCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using WakeCommerce.ApiService.Extenssions;
using Serilog;
using Serilog.Events;
using WakeCommerce.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Definindo o environment a partir de uma variável de ambiente ou parâmetro customizado
var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"Ambiente:{environment}");

// Configurando o appsettings específico com base no environment
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
.AddEnvironmentVariables();

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console(LogEventLevel.Debug)
    .WriteTo.File($"log-{DateTime.UtcNow.ToString("dd/MM/yyyy")}.txt",
        LogEventLevel.Warning,
        rollingInterval: RollingInterval.Day));

builder.Services.AddDependencies(builder);


// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

var app = builder.Build();

// Aplica as migrações pendentes
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WakeCommerceDbContext>();

    // Aplica as migrações e, em seguida, faz a carga de produtos
    dbContext.Database.Migrate();  // Aplica migrações pendentes
    dbContext.UploadProdutos();  // Faz a carga de produtos
}

//add opentelemetry exporter prometheus 
app.UseOpenTelemetryPrometheusScrapingEndpoint();

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

if (app.Environment.IsDevelopment() || environment == "Aspire")
{
    app.MapOpenApi();
}

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();

public partial class Program { }