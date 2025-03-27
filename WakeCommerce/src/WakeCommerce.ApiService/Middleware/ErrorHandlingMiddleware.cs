
using WakeCommerce.ApiService.Controllers.Base;
using Serilog;
using System.Diagnostics;
using OpenTelemetry.Trace;
using Serilog.Context;

namespace WakeCommerce.ApiService.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                var traceId = Activity.Current?.TraceId.ToString() ?? "no-trace-id";

                var teste = context.Request.HttpContext.TraceIdentifier;

                LogContext.PushProperty("TraceId", traceId);

                await _next(context);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro não tratado ocorreu na requisição {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                Activity.Current?.SetStatus(ActivityStatusCode.Error);
                Activity.Current?.RecordException(ex, new TagList
                {
                    {"system.erro", ex.Message }
                });

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                await context.Response.WriteAsJsonAsync(new ProblemDatails<List<string>>()
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Erro interno do servidor, contate o administrador.",
                    Detail = new List<string> { ex.Message }
                });
            }
        }
    }
}
