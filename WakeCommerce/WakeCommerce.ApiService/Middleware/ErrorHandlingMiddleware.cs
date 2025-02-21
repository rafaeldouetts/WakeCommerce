using Microsoft.AspNetCore.Mvc;
using WakeCommerce.ApiService.Controllers.Base;
using Serilog;

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
                await _next(context);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro não tratado ocorreu na requisição {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                await context.Response.WriteAsJsonAsync(new ProblemDatails()
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Erro interno do servidor, contate o administrador.",
                    Detail = ex.Message
                });
            }
        }
    }
}
