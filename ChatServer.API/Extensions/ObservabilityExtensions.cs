using MassTransit.Logging;
using MassTransit.Monitoring;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ChatServer.API.Extensions;

public static class ObservabilityExtensions
{
    public static void AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(
                    serviceName: "cypherly.chatserver.svc",
                    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString(),
                    serviceInstanceId: Environment.MachineName))
            .WithTracing(b => b
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddRedisInstrumentation()
                .AddQuartzInstrumentation()
                .AddNpgsql()
                .AddSource(DiagnosticHeaders.DefaultListenerName)
                .AddOtlpExporter())
            .WithMetrics(b => b
                .AddRuntimeInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter(InstrumentationOptions.MeterName)
                .AddOtlpExporter());
    }

    public static void AddLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.AddOtlpExporter();
        });

        if (builder.Environment.IsDevelopment())
        {
            builder.Logging.AddSimpleConsole(o =>
            {
                o.SingleLine = true;
                o.TimestampFormat = "HH:mm:ss ";
                o.IncludeScopes = true;
            });
        }
    }
}


