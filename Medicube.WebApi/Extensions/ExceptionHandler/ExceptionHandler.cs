using System.Net;
using System.Text.Json;

namespace WebApi.Extensions.ExceptionHandler
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                context.Request.EnableBuffering();
                string requestBody = string.Empty;
                if (context.Request.ContentLength > 0)
                {
                    using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
                    {
                        requestBody = await reader.ReadToEndAsync();
                        context.Request.Body.Position = 0;
                    }
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");

                await LogExceptionToFileAsync(context, ex);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                success = false,
                message = "An unexpected error occurred. Please try again later.",
                error = exception.Message,
                innerException =  exception.InnerException == null ? "Null" : exception.InnerException.Message,
                stackTrace = exception.StackTrace
            };

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }

        private static async Task LogExceptionToFileAsync(HttpContext context, Exception exception)
        {
            try
            {
                string logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                string logFile = Path.Combine(logDir, $"Exception_{DateTime.UtcNow:yyyyMMdd}.log");

                string method = context.Request.Method;
                string path = context.Request.Path;
                string query = context.Request.QueryString.ToString();

                context.Request.EnableBuffering();
                string requestBody = string.Empty;
                if (context.Request.ContentLength > 0)
                {
                    using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
                    {
                        requestBody = await reader.ReadToEndAsync();
                        context.Request.Body.Position = 0;
                    }
                }

                var logEntry = new
                {
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    Method = method,
                    Path = path,
                    Query = query,
                    RequestBody = requestBody,
                    ExceptionMessage = exception.Message,
                    StackTrace = exception.StackTrace
                };

                string logText = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = true });
                await File.AppendAllTextAsync(logFile, logText + Environment.NewLine + new string('-', 80) + Environment.NewLine);
            }
            catch
            {
                // Don't throw from logging
            }
        }
    }
}
