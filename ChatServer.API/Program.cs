using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using ChatServer.API.Extensions;
using ChatServer.API.Hubs;
using ChatServer.Application.Extensions;
using ChatServer.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

#pragma warning disable CS0618 // Type or member is obsolete


var builder = WebApplication.CreateBuilder(args);

#region Extensions

var env = builder.Environment;

var configuration = builder.Configuration;
configuration.AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables();

if (env.IsDevelopment())
{
    configuration.AddJsonFile($"appsettings.{Environments.Development}.json", true, true);
    configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
}

#endregion

#region Logger

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

builder.Services.AddObservability(configuration);

builder.Host.UseSerilog();

#endregion

#region Application Layer

builder.Services.AddApplication(Assembly.Load("ChatServer.Application"));

#endregion

# region Infrastructure Layer

builder.Services.AddInfrastructure(configuration, Assembly.Load("ChatServer.Infrastructure"));

#endregion

#region SignalR Extensions

builder.Services.ConfigureSignalR();

#endregion

#region Authentication & Authorization

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["Jwt:Issuer"] ?? throw new NotImplementedException($"MISSING VALUE IN JWT SETTINGS {configuration["Jwt:Issuer"]}"),
        ValidAudience = configuration["Jwt:Audience"] ?? throw new NotImplementedException($"MISSING VALUE IN JWT SETTINGS {configuration["Jwt:Audience"]}"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"] ??
                                                                           throw new NotImplementedException("MISSING VALUE IN JWT SETTINGS Jwt:Secret"))),
    };

    options.Events = new JwtBearerEvents()
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs") ||
                 path.StartsWithSegments("/change-eventhub") ||
                 path.StartsWithSegments("/message-hub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },

        OnTokenValidated = context =>
        {
            Log.Information("Successfully validated token for: {User}",
                context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Log.Information("Authentication failed: {Exception}",
                context.Exception);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

#endregion

#region CORS

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

#endregion

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (env.IsProduction())
{
    app.Services.ApplyPendingMigrations();
}

app.UseCors("AllowElectron");

app.UseAuthentication();

app.UseAuthorization();

Log.Information("Chat Server booted up");

app.MapPrometheusScrapingEndpoint();
app.UseSerilogRequestLogging();

app.MapHub<ChangeEventHub>("change-eventhub");
app.MapHub<MessageHub>("message-hub");

app.Run();

public partial class Program
{
}