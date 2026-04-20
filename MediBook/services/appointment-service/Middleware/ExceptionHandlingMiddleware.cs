using System.Net;
using System.Text.Json;

namespace appointment_service.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = exception switch
            {
                InvalidOperationException => (int)HttpStatusCode.BadRequest,
                ArgumentException         => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException      => (int)HttpStatusCode.NotFound,
                _                         => (int)HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = statusCode;

            var result = JsonSerializer.Serialize(new
            {
                error      = exception.Message,
                statusCode = statusCode
            });

            return context.Response.WriteAsync(result);
        }
    }
}
