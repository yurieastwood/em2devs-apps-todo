using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Infrastructure.Persistence;
using EM2Devs.Todo.ServiceDefaults;
using EM2Devs.Todo.Worker.Jobs;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

// Aspire-provided DbContext via the "tododb" connection string.
builder.AddNpgsqlDbContext<TodoDbContext>("tododb");

// Background jobs run outside of any HTTP request — provide a system-level
// ICurrentUser so task-scoped repositories can still filter by UserId.
builder.Services.AddScoped<ICurrentUser, SystemCurrentUser>();

// Postgres-backed repositories.
builder.Services.AddScoped<ITaskRepository, PostgresTaskRepository>();
builder.Services.AddScoped<IRecurringTaskRepository, PostgresRecurringTaskRepository>();
builder.Services.AddScoped<IPlayerProfileRepository, PostgresPlayerProfileRepository>();
builder.Services.AddScoped<IStreakSnapshotRepository, PostgresStreakSnapshotRepository>();
builder.Services.AddScoped<INotificationRepository, PostgresNotificationRepository>();

// Singleton cache shared across scoped repo instances for the "last XP breakdown" UI hint.
builder.Services.AddSingleton<ILastXpBreakdownCache, LastXpBreakdownCache>();

// TimeProvider for deterministic test substitution.
builder.Services.AddSingleton(TimeProvider.System);

// Quartz scheduler with two cron jobs.
builder.Services.AddQuartz(q =>
{
    q.SchedulerName = "EM2Devs.Todo.Worker";

    JobKey generationKey = new("RecurringTaskGenerationJob");
    q.AddJob<RecurringTaskGenerationJob>(opts => opts.WithIdentity(generationKey));
    q.AddTrigger(opts => opts
        .ForJob(generationKey)
        .WithIdentity("RecurringTaskGenerationTrigger")
        .WithCronSchedule("0 0/5 * * * ?")); // every 5 minutes

    JobKey streakKey = new("DailyStreakEvaluationJob");
    q.AddJob<DailyStreakEvaluationJob>(opts => opts.WithIdentity(streakKey));
    q.AddTrigger(opts => opts
        .ForJob(streakKey)
        .WithIdentity("DailyStreakEvaluationTrigger")
        .WithCronSchedule("0 0 0 * * ?", x => x.InTimeZone(TimeZoneInfo.Utc))); // 00:00 UTC daily
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

var host = builder.Build();
host.Run();
