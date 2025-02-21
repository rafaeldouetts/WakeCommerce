using System.Net;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using WakeCommerce.ApiService.Middleware;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

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

// Configure the HTTP request pipeline.
//app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();