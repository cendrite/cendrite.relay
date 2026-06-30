using System.Reflection;
using ChatServer.API.Extensions;
using ChatServer.API.Hubs;
using ChatServer.Application.Extensions;
using ChatServer.Infrastructure.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

var configuration = builder.Configuration;
configuration.AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables();

if (env.IsDevelopment())
{

    configuration.AddJsonFile($"appsettings.{Environments.Development}.json", true, true);
    configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
}

builder.AddLogging();
builder.Services.AddObservability(configuration);

builder.Services.AddApplication(Assembly.Load("ChatServer.Application"));

builder.Services.AddInfrastructure(configuration, Assembly.Load("ChatServer.Infrastructure"));

builder.Services.AddAuthentication(configuration);

builder.Services.ConfigureSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowElectron", policy =>
    {
        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();
        policy.WithOrigins(allowedOrigins!)
            .AllowAnyMethod()
            .AllowCredentials()
            .AllowAnyHeader();
    });
});

builder.Services.AddEndpoints();

builder.Services.AddOpenApi();

var app = builder.Build();

app.RegisterMinimalEndpoints();

if (env.IsProduction())
{
    app.Services.ApplyPendingMigrations();
}

app.UseCors("AllowElectron");


app.MapOpenApi();

app.MapScalarApiReference(options =>
{
    options.WithTitle("Keystore.API V1")
        .WithTheme(ScalarTheme.Purple)
        .HideDarkModeToggle()
        .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios);
});

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChangeEventHub>("change-eventhub");
app.MapHub<MessageHub>("message-hub");

var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Starting ChatServer API");
    app.Run();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Application terminated unexpectedly");
}

public partial class Program
{
}
