using CourseHub.Core.Data;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("CourseHub");
if (string.IsNullOrWhiteSpace(connectionString) || !HasDbPassword(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:CourseHub is missing a password. Set it with dotnet user-secrets. See the README.");
}

builder.Services.AddDbContext<CourseHubDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new { error = error?.Message });
}));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CourseHubDbContext>();
    db.Database.EnsureCreated();
    DbSeeder.Seed(db);
}

app.MapControllers();

app.Run();

static bool HasDbPassword(string connectionString)
{
    foreach (var part in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
    {
        const string prefix = "Password=";
        if (part.Trim().StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
            part.Trim().Length > prefix.Length)
        {
            return true;
        }
    }

    return false;
}
