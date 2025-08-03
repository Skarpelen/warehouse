using System.Text;
using Microsoft.AspNetCore.Http;
using NLog;

namespace Warehouse.BusinessLogic.Middlewares
{
    using Warehouse.BusinessLogic.Models;

    public class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                if (ex is SecureException secureException)
                {
                    var queryParameters = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "N/A";
                    var bodyParameters = await GetRequestBodyAsync(context);
                    secureException.QueryParameters = queryParameters;
                    secureException.BodyParameters = bodyParameters;
                    await HandleExceptionAsync(context, secureException);
                }
                else
                {
                    await HandleExceptionAsync(context, ex);
                }
            }
        }

        private async Task<string> GetRequestBodyAsync(HttpContext context)
        {
            if (context.Request.ContentLength > 0)
            {
                context.Request.Body.Position = 0;

                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    var body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                    return body;
                }
            }

            return "N/A";
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var eventId = DateTime.UtcNow.Ticks;
            var response = new { id = eventId.ToString() };
            context.Response.ContentType = "application/json";

            if (exception is SecureException secureException)
            {
                await LogExceptionToJournal(eventId, secureException, "Secure");

                context.Response.StatusCode = 500;
                var secureResponse = new
                {
                    type = "Secure",
                    id = eventId,
                    data = new { message = secureException.Message }
                };

                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(secureResponse));
            }
            else
            {
                await LogExceptionToJournal(eventId, exception, "Exception");

                context.Response.StatusCode = 500;
                var exceptionResponse = new
                {
                    type = "Exception",
                    id = eventId,
                    data = new { message = $"Internal server error ID = {eventId}" }
                };

                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(exceptionResponse));
            }

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }

        // Тут можно сделать логику передачи лога в какой то журнал, например ES
        private Task LogExceptionToJournal(long eventId, Exception exception, string type)
        {
            var sb = new StringBuilder();

            sb.AppendLine("[ErrorLog]");
            sb.AppendLine($"EventId: {eventId}");
            sb.AppendLine($"Timestamp: {DateTime.UtcNow:o}");
            sb.AppendLine($"ExceptionType: {type}");
            sb.AppendLine($"Message: {exception.Message}");
            sb.AppendLine($"StackTrace: {exception.StackTrace ?? "N/A"}");

            if (exception is SecureException secureEx)
            {
                sb.AppendLine($"QueryParameters: {secureEx.QueryParameters}");
                sb.AppendLine($"BodyParameters: {secureEx.BodyParameters}");
            }
            else
            {
                sb.AppendLine("QueryParameters: N/A");
                sb.AppendLine("BodyParameters: N/A");
            }

            _log.Error(sb.ToString());

            return Task.CompletedTask;
        }
    }
}
