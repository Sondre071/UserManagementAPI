using System.Diagnostics;

namespace UserManagementAPI.Middleware;

public class LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Log request details
        logger.LogInformation("Incoming request: {Method} {Path}", context.Request.Method, context.Request.Path);

        await next(context); // Call the next middleware

        stopwatch.Stop();

        // Log response details
        logger.LogInformation("Response: {StatusCode} | Duration: {ElapsedMilliseconds}ms",
            context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
    }
}