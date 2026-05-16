using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ChatServer.API.Extensions;

public static class AuthenticationExtensions
{
    public static void AddAuthentication(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        var authority = configuration["Jwt:Issuer"] 
            ?? throw new NotImplementedException("MISSING VALUE IN JWT SETTINGS Jwt:Issuer");

        var audience = configuration["Jwt:Audience"] 
            ?? throw new NotImplementedException("MISSING VALUE IN JWT SETTINGS Jwt:Audience");

        services.AddAuthentication().AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.Audience = audience;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = authority,
                ValidAudience = audience,
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
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Token validated for {User}", context.Principal?.Identity?.Name);
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(context.Exception, "Authentication failed");
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();
    }
}
