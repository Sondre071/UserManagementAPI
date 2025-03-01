using System.Text.RegularExpressions;
using UserManagementAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();


// Error handling
app.UseMiddleware<ErrorHandlingMiddleware>();

// Mock authentication token for testing. Add 'bearer give-me-access' to your request to gain access.
app.UseMiddleware<MockAuthentication>();

// Logging
app.UseMiddleware<LoggingMiddleware>();


// Log incoming requests and outgoing responses.
app.Use(async (context, next) =>
{
    var request = context.Request;
    Console.WriteLine($"Incoming Request: {request.Method} {request.Path}");

    await next();

    var response = context.Response;
    Console.WriteLine($"Outgoing Response: {response.StatusCode}");
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();

// Mock user data
var users = new List<User>
{
    new User { Id = 1, Name = "John Doe", Email = "john.doe@example.com" },
    new User { Id = 2, Name = "Jane Smith", Email = "jane.smith@example.com" }
};

var userDict = users.ToDictionary(u => u.Id);

Regex emailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

// GET: Retrieve all users
app.MapGet("/users", () =>
{
    try
    {
        return Results.Ok(users);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Internal server error: {ex.Message}");
    }
});

// GET: Retrieve a specific user by ID
app.MapGet("/users/{id}", (int id) =>
{
    try
    {
        return userDict.TryGetValue(id, out var user)
            ? Results.Ok(user)
            : Results.NotFound("User not found");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error retrieving user: {ex.Message}");
    }
});

// POST: Add a new user
app.MapPost("/users", (User newUser) =>
{
    try
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(newUser.Name) || !emailRegex.IsMatch(newUser.Email))
        {
            return Results.BadRequest("Invalid user data. Name cannot be empty, and email must be valid.");
        }

        newUser.Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
        users.Add(newUser);
        userDict[newUser.Id] = newUser;

        return Results.Created($"/users/{newUser.Id}", newUser);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error adding user: {ex.Message}");
    }
});

// PUT: Update an existing user
app.MapPut("/users/{id}", (int id, User updatedUser) =>
{
    try
    {
        if (!userDict.TryGetValue(id, out var user))
        {
            return Results.NotFound("User not found");
        }

        // Validate input
        if (string.IsNullOrWhiteSpace(updatedUser.Name) || !emailRegex.IsMatch(updatedUser.Email))
        {
            return Results.BadRequest("Invalid user data. Name cannot be empty, and email must be valid.");
        }

        user.Name = updatedUser.Name;
        user.Email = updatedUser.Email;

        return Results.Ok(user);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error updating user: {ex.Message}");
    }
});

// DELETE: Remove a user by ID
app.MapDelete("/users/{id}", (int id) =>
{
    try
    {
        if (!userDict.TryGetValue(id, out var user))
        {
            return Results.NotFound("User not found");
        }

        users.Remove(user);
        userDict.Remove(id);

        return Results.Ok($"User with ID {id} deleted");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error deleting user: {ex.Message}");
    }
});

app.Run();

// User model
record User
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}