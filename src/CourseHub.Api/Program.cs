using CourseHub.Api.Auth;
using CourseHub.Core.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CourseOwner", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new CourseOwnerRequirement());
    });
});
builder.Services.AddSingleton<IAuthorizationHandler, CourseOwnerHandler>();

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
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var traceId = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
        .CreateLogger("CourseHub.Api.Exceptions");
    logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);

    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new
    {
        error = "An unexpected error occurred.",
        traceId
    });
}));

app.UseAuthentication();
app.UseAuthorization();

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
