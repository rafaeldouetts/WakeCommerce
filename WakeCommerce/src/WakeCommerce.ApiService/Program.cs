using Scalar.AspNetCore;
using WakeCommerce.ApiService.Middleware;
using WakeCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using WakeCommerce.ApiService.Extenssions;
using Serilog;
using WakeCommerce.ServiceDefaults;
using Prometheus;
using Serilog.Enrichers.Span;

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

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
    .Enrich.WithSpan(new SpanOptions() { IncludeOperationName = true, IncludeTags = true }));

builder.Services.AddDependencies(builder);


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

app.MapDefaultEndpoints();

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


app.UseMetricServer();
app.UseHttpMetrics();

//app.UseSerilogRequestLogging();

app.Run();

public partial class Program { }