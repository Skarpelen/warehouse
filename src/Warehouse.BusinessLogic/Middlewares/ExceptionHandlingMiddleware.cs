using Microsoft.AspNetCore.Http;
using NLog;
using System.Text;
using System.Text.Json;

namespace Warehouse.BusinessLogic.Middlewares
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            context.Request.EnableBuffering();

            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var eventId = DateTime.UtcNow.Ticks;

            var sb = new StringBuilder();
            sb.AppendLine("[ErrorLog]")
              .AppendLine($"EventId: {eventId}")
              .AppendLine($"Timestamp: {DateTime.UtcNow:o}");

            var ex = exception;

            while (ex != null)
            {
                sb.AppendLine($"ExceptionType: {ex.GetType().FullName}");
                sb.AppendLine($"Message: {ex.Message}");
                sb.AppendLine($"StackTrace: {ex.StackTrace ?? "N/A"}");
                ex = ex.InnerException;
            }

            sb.AppendLine(context.Request.QueryString.HasValue
                ? $"QueryString: {context.Request.QueryString}"
                : "QueryString: N/A");

            _log.Error(sb.ToString());

            var resp = new
            {
                type = "Exception",
                id = eventId,
                data = new
                {
                    message = $"Internal server error ID = {eventId}"
                }
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(JsonSerializer.Serialize(resp));
        }
    }
}
