namespace UserManagementAPI.Middleware;

public class MockAuthentication(RequestDelegate next)
{
    private const string ValidToken = "give-me-access";

    public async Task Invoke(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (authHeader == null || !authHeader.StartsWith("Bearer ") || authHeader.Split(" ")[1] != ValidToken)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Invalid or missing token.");
            return;
        }

        await next(context); // Token is valid, continue request processing
    }
}