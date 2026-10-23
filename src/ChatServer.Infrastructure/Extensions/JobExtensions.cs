using System.Reflection;
using ChatServer.Infrastructure.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace ChatServer.Infrastructure.Extensions;

internal static class JobExtensions
{
    private const string ConnectionStringName = "ChatServerDbConnectionString";

    internal static void AddJobs(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
    {
        services.AddQuartz(configure =>
        {
            configure.SchedulerId = "AUTO";

            var outboxJobKey = new JobKey($"{nameof(ProcessOutboxMessageJob)}-{assembly.GetName()}");

            configure.AddJob<ProcessOutboxMessageJob>(outboxJobKey)
                .AddTrigger(trigger => trigger.ForJob(outboxJobKey)
                    .WithSimpleSchedule(schedule => schedule.WithIntervalInSeconds(10).RepeatForever()));

            var mediaRetentionJobKey = new JobKey($"{nameof(MediaRetentionJob)}-{assembly.GetName()}");

            configure.AddJob<MediaRetentionJob>(mediaRetentionJobKey)
                .AddTrigger(trigger => trigger.ForJob(mediaRetentionJobKey)
                    .StartNow()
                    .WithSimpleSchedule(schedule => schedule.WithIntervalInHours(1).RepeatForever()));

            configure.UsePersistentStore(store =>
            {
                store.UsePostgres(options =>
                {
                    options.ConnectionString = configuration.GetConnectionString(ConnectionStringName)
                        ?? throw new InvalidOperationException($"Missing {ConnectionStringName} connection string.");
                });

                store.UseSystemTextJsonSerializer();
                store.UseClustering();
            });
        });

        services.AddQuartzHostedService();
    }
}
