using System.Text;
using System.Text.Json.Serialization;
using EM2Devs.Todo.Api.ModelBinding;
using Scalar.AspNetCore;
using EM2Devs.Todo.Application.Behaviors;
using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Application.Validators;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Infrastructure.Auth;
using EM2Devs.Todo.Infrastructure.Persistence;
using EM2Devs.Todo.ServiceDefaults;
using EM2Devs.Todo.Api.Extensions;
using EM2Devs.Todo.Api.Hubs;
using EM2Devs.Todo.Api.Middleware;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

const string CorsPolicyName = "Frontend";

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AllowResponseHeaderCompression = true;
    options.ConfigureEndpointDefaults(listen => listen.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2);
    options.AddServerHeader = false;
});
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.RequestHeaderEncodingSelector = _ => System.Text.Encoding.Latin1;
});

builder.AddServiceDefaults();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

string? connectionString = builder.Configuration.GetConnectionString("tododb");
builder.Services.AddSingleton<ILastXpBreakdownCache, LastXpBreakdownCache>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpContextAccessor();

// Always register BCryptPasswordHasher as a singleton (pure, stateless).
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

if (!string.IsNullOrEmpty(connectionString))
{
    // Suppress PendingModelChangesWarning: the model snapshot doesn't exactly match the
    // runtime model for Phase 0–3 entities added outside of `dotnet ef migrations add`.
    // The hand-written AddPhase0to3TaskColumns migration creates the columns correctly;
    // chasing the snapshot-model diff is technical debt for a later pass.
    builder.AddNpgsqlDbContext<TodoDbContext>("tododb", configureDbContextOptions: options =>
    {
        options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    });
    builder.Services.AddScoped<ITaskRepository, PostgresTaskRepository>();
    builder.Services.AddScoped<IRecurringTaskRepository, PostgresRecurringTaskRepository>();
    builder.Services.AddScoped<IPlayerProfileRepository, PostgresPlayerProfileRepository>();
    builder.Services.AddScoped<IStreakSnapshotRepository, PostgresStreakSnapshotRepository>();
    builder.Services.AddScoped<IUserRepository, PostgresUserRepository>();
    builder.Services.AddScoped<INotificationRepository, PostgresNotificationRepository>();
}
else
{
    // Scoped so it can depend on the scoped ICurrentUser; state lives in the singleton store.
    builder.Services.AddSingleton<InMemoryTaskStore>();
    builder.Services.AddScoped<ITaskRepository, InMemoryTaskRepository>();
    // Slice 2: scoped recurring task repo with singleton store, mirrors the task pattern
    builder.Services.AddSingleton<InMemoryRecurringTaskStore>();
    builder.Services.AddScoped<IRecurringTaskRepository, InMemoryRecurringTaskRepository>();
    // Slice 3: scoped player profile repo with singleton store, per-user via ICurrentUser
    builder.Services.AddSingleton<InMemoryPlayerProfileStore>();
    builder.Services.AddScoped<IPlayerProfileRepository, InMemoryPlayerProfileRepository>();
    builder.Services.AddSingleton<IUserRepository>(sp =>
        new InMemoryUserRepository(sp.GetRequiredService<IPasswordHasher>()));
    // Notifications: scoped repo with singleton store, mirrors the task pattern
    builder.Services.AddSingleton<InMemoryNotificationStore>();
    builder.Services.AddScoped<INotificationRepository, InMemoryNotificationRepository>();
}

// TODO: Add conditional Postgres/InMemory registration for Quest/Epic repositories when their persistence implementations are added
builder.Services.AddSingleton<IQuestRepository, InMemoryQuestRepository>();
builder.Services.AddSingleton<IEpicRepository, InMemoryEpicRepository>();

// Weekly review reflections: in-memory only for this slice. Persistence is keyed by
// (UserId, WeekOf) — the singleton store survives across scoped repositories.
builder.Services.AddSingleton<InMemoryWeeklyReflectionStore>();
builder.Services.AddScoped<IWeeklyReflectionRepository, InMemoryWeeklyReflectionRepository>();

// JWT-backed ICurrentUser reads HttpContext.User claims on each request.
builder.Services.AddScoped<ICurrentUser, JwtCurrentUser>();

builder.Services.AddScoped<IMediator, Mediator>();

// CQRS handlers (return Result<T> per ADR-018)
builder.Services.AddTransient<IRequestHandler<CreateTaskCommand, Result<TodoTask>>, CreateTaskCommandHandler>();
builder.Services.AddTransient<IRequestHandler<QuickAddTaskCommand, Result<TodoTask>>, QuickAddTaskCommandHandler>();
builder.Services.AddTransient<IRequestHandler<UpdateTaskStatusCommand, Result<TodoTask>>, UpdateTaskStatusCommandHandler>();
builder.Services.AddTransient<IRequestHandler<UpdateTaskCommand, Result<TodoTask>>, UpdateTaskCommandHandler>();
builder.Services.AddTransient<IRequestHandler<ReopenTaskCommand, Result<TodoTask>>, ReopenTaskCommandHandler>();
builder.Services.AddTransient<IRequestHandler<RecordActualTimeCommand, Result<TodoTask>>, RecordActualTimeCommandHandler>();
builder.Services.AddTransient<IRequestHandler<DeleteTaskCommand, Result<bool>>, DeleteTaskCommandHandler>();
builder.Services.AddTransient<IRequestHandler<GetTaskQuery, Result<TodoTask>>, GetTaskQueryHandler>();
builder.Services.AddTransient<IRequestHandler<ListTasksQuery, Result<IReadOnlyList<TodoTask>>>, ListTasksQueryHandler>();

// Quest CQRS handlers
builder.Services.AddTransient<IRequestHandler<CreateQuestCommand, Result<Quest>>, CreateQuestCommandHandler>();
builder.Services.AddTransient<IRequestHandler<AddTaskToQuestCommand, Result<Quest>>, AddTaskToQuestCommandHandler>();
builder.Services.AddTransient<IRequestHandler<RemoveTaskFromQuestCommand, Result<Quest>>, RemoveTaskFromQuestCommandHandler>();
builder.Services.AddTransient<IRequestHandler<CompleteQuestCommand, Result<Quest>>, CompleteQuestCommandHandler>();
builder.Services.AddTransient<IRequestHandler<DeleteQuestCommand, Result<bool>>, DeleteQuestCommandHandler>();
builder.Services.AddTransient<IRequestHandler<GetQuestQuery, Result<Quest>>, GetQuestQueryHandler>();
builder.Services.AddTransient<IRequestHandler<ListQuestsQuery, Result<IReadOnlyList<Quest>>>, ListQuestsQueryHandler>();

// Epic CQRS handlers
builder.Services.AddTransient<IRequestHandler<CreateEpicCommand, Result<Epic>>, CreateEpicCommandHandler>();
builder.Services.AddTransient<IRequestHandler<AssignQuestToEpicCommand, Result<Epic>>, AssignQuestToEpicCommandHandler>();
builder.Services.AddTransient<IRequestHandler<RemoveQuestFromEpicCommand, Result<Epic>>, RemoveQuestFromEpicCommandHandler>();
builder.Services.AddTransient<IRequestHandler<CompleteEpicCommand, Result<Epic>>, CompleteEpicCommandHandler>();
builder.Services.AddTransient<IRequestHandler<DeleteEpicCommand, Result<bool>>, DeleteEpicCommandHandler>();
builder.Services.AddTransient<IRequestHandler<GetEpicQuery, Result<Epic>>, GetEpicQueryHandler>();
builder.Services.AddTransient<IRequestHandler<ListEpicsQuery, Result<IReadOnlyList<Epic>>>, ListEpicsQueryHandler>();

// Recurring task CQRS handlers
builder.Services.AddTransient<IRequestHandler<CreateRecurringTaskCommand, Result<RecurringTask>>, CreateRecurringTaskCommandHandler>();
builder.Services.AddTransient<IRequestHandler<GenerateInstancesCommand, Result<TodoTask>>, GenerateInstancesCommandHandler>();
builder.Services.AddTransient<IRequestHandler<UpdateRecurringTaskCommand, Result<RecurringTask>>, UpdateRecurringTaskCommandHandler>();
builder.Services.AddTransient<IRequestHandler<PauseRecurringTaskCommand, Result<RecurringTask>>, PauseRecurringTaskCommandHandler>();
builder.Services.AddTransient<IRequestHandler<ResumeRecurringTaskCommand, Result<RecurringTask>>, ResumeRecurringTaskCommandHandler>();
builder.Services.AddTransient<IRequestHandler<DeleteRecurringTaskCommand, Result<bool>>, DeleteRecurringTaskCommandHandler>();
builder.Services.AddTransient<IRequestHandler<GetRecurringTaskQuery, Result<RecurringTask>>, GetRecurringTaskQueryHandler>();
builder.Services.AddTransient<IRequestHandler<ListRecurringTasksQuery, Result<IReadOnlyList<RecurringTask>>>, ListRecurringTasksQueryHandler>();
builder.Services.AddTransient<IRequestHandler<ListRecurringTaskInstancesQuery, Result<IReadOnlyList<TodoTask>>>, ListRecurringTaskInstancesQueryHandler>();

// Auth CQRS handlers (Phase 0 multi-user JWT).
builder.Services.AddTransient<IRequestHandler<RegisterUserCommand, Result<LoginResult>>, RegisterUserCommandHandler>();
builder.Services.AddTransient<IRequestHandler<LoginCommand, Result<LoginResult>>, LoginCommandHandler>();

// Focus mode commands (Boss Task focus session).
builder.Services.AddTransient<IRequestHandler<StartFocusModeCommand, Result<bool>>, StartFocusModeCommandHandler>();
builder.Services.AddTransient<IRequestHandler<EndFocusModeCommand, Result<FocusModeResult>>, EndFocusModeCommandHandler>();

// Profile query handler (Phase 3 profile expansion).
builder.Services.AddTransient<IRequestHandler<GetPlayerProfileQuery, Result<PlayerProfileReadModel>>, GetPlayerProfileQueryHandler>();
builder.Services.AddTransient<IRequestHandler<FreezeStreakCommand, Result<PlayerProfileReadModel>>, FreezeStreakCommandHandler>();

// Calendar service (null implementation — swap for real integration when available).
builder.Services.AddSingleton<ICalendarService, EM2Devs.Todo.Infrastructure.Calendar.NullCalendarService>();

// Daily brief query handler (stateless — recomputed on each call).
builder.Services.AddTransient<IRequestHandler<GetDailyBriefQuery, Result<DailyBriefReadModel>>, GetDailyBriefQueryHandler>();

// Estimation calibration query handler (stateless — recomputed from task history).
builder.Services.AddTransient<IRequestHandler<GetEstimationBiasQuery, Result<EstimationCalibrationReadModel>>, GetEstimationBiasQueryHandler>();
builder.Services.AddTransient<IRequestHandler<GetEstimationDashboardQuery, Result<EstimationDashboardReadModel>>, GetEstimationDashboardQueryHandler>();

// Weekly review handlers.
builder.Services.AddTransient<IRequestHandler<GetWeeklyReviewQuery, Result<WeeklyReviewReadModel>>, GetWeeklyReviewQueryHandler>();
builder.Services.AddTransient<IRequestHandler<SaveWeeklyReviewCommand, Result<WeeklyReflectionReadModel>>, SaveWeeklyReviewCommandHandler>();

builder.Services.AddTransient<INotificationHandler<EM2Devs.Todo.Application.Events.TaskCompletedEvent>,
    EM2Devs.Todo.Application.Events.XpAwardHandler>();
builder.Services.AddTransient<INotificationHandler<EM2Devs.Todo.Application.Events.TaskStatusChangedEvent>,
    EM2Devs.Todo.Application.Events.QuestProgressHandler>();
builder.Services.AddTransient<INotificationHandler<EM2Devs.Todo.Application.Events.TaskDeletedEvent>,
    EM2Devs.Todo.Application.Events.TaskDeletedHandler>();
builder.Services.AddTransient<INotificationHandler<EM2Devs.Todo.Application.Events.QuestCompletedEvent>,
    EM2Devs.Todo.Application.Events.QuestCompletionXpHandler>();

// Onboarding state query handler.
builder.Services.AddTransient<IRequestHandler<GetOnboardingStateQuery, Result<OnboardingStateReadModel>>, GetOnboardingStateQueryHandler>();

// Seasons query handler.
builder.Services.AddTransient<IRequestHandler<GetCurrentSeasonQuery, Result<CurrentSeasonReadModel>>, GetCurrentSeasonQueryHandler>();

// Annual wrapped query handler.
builder.Services.AddTransient<IRequestHandler<GetAnnualWrappedQuery, Result<AnnualWrappedReadModel>>, GetAnnualWrappedQueryHandler>();

// Subscription query handler.
builder.Services.AddTransient<IRequestHandler<GetSubscriptionQuery, Result<SubscriptionReadModel>>, GetSubscriptionQueryHandler>();

// Capacity modelling query handler.
builder.Services.AddTransient<IRequestHandler<GetCapacityOverviewQuery, Result<CapacityOverviewReadModel>>, GetCapacityOverviewQueryHandler>();

// Procrastination detection query handler.
builder.Services.AddTransient<IRequestHandler<GetProcrastinationCandidatesQuery, Result<IReadOnlyList<ProcrastinationCandidateReadModel>>>, GetProcrastinationCandidatesQueryHandler>();

// Insight cards: in-memory store for personalised productivity insights.
builder.Services.AddSingleton<InMemoryInsightCardStore>();
builder.Services.AddScoped<IInsightCardRepository, InMemoryInsightCardRepository>();
builder.Services.AddTransient<IRequestHandler<ListInsightCardsQuery, Result<IReadOnlyList<InsightCardReadModel>>>, ListInsightCardsQueryHandler>();
builder.Services.AddTransient<IRequestHandler<MarkInsightReadCommand, Result<bool>>, MarkInsightReadCommandHandler>();
builder.Services.AddTransient<IRequestHandler<SaveInsightCommand, Result<bool>>, SaveInsightCommandHandler>();
builder.Services.AddTransient<IRequestHandler<DismissInsightCommand, Result<bool>>, DismissInsightCommandHandler>();

// Energy check-in: in-memory store for energy level tracking.
builder.Services.AddSingleton<InMemoryEnergyCheckInStore>();
builder.Services.AddScoped<IEnergyCheckInRepository, InMemoryEnergyCheckInRepository>();
builder.Services.AddTransient<IRequestHandler<EnergyCheckInCommand, Result<EnergyCheckInResult>>, EnergyCheckInCommandHandler>();
builder.Services.AddTransient<IRequestHandler<GetEnergyProfileQuery, Result<EnergyProfileReadModel>>, GetEnergyProfileQueryHandler>();

// Timeline: in-memory store for journey timeline events.
builder.Services.AddSingleton<InMemoryTimelineStore>();
builder.Services.AddScoped<ITimelineRepository, InMemoryTimelineRepository>();

// Timeline query handler.
builder.Services.AddTransient<IRequestHandler<GetTimelineQuery, Result<TimelineReadModel>>, GetTimelineQueryHandler>();

// Timeline event creation: populate timeline on level-up, streak milestone, quest completion.
// Title evaluation: check title eligibility after each task completion.
builder.Services.AddTransient<INotificationHandler<EM2Devs.Todo.Application.Events.TaskCompletedEvent>,
    EM2Devs.Todo.Application.Events.TitleEvaluationHandler>();

builder.Services.AddTransient<INotificationHandler<EM2Devs.Todo.Application.Events.LevelUpEvent>,
    EM2Devs.Todo.Application.Events.TimelineRecordingHandler>();
builder.Services.AddTransient<INotificationHandler<EM2Devs.Todo.Application.Events.StreakMilestoneReachedEvent>,
    EM2Devs.Todo.Application.Events.TimelineRecordingHandler>();
builder.Services.AddTransient<INotificationHandler<EM2Devs.Todo.Application.Events.QuestCompletedEvent>,
    EM2Devs.Todo.Application.Events.TimelineRecordingHandler>();

// Skill tree discovery: evaluate tag-based thresholds after each task completion.
builder.Services.AddTransient<INotificationHandler<EM2Devs.Todo.Application.Events.TaskCompletedEvent>,
    EM2Devs.Todo.Application.Events.SkillTreeDiscoveryHandler>();

// Surface achievement-style events in the in-app notifications inbox.
builder.Services.AddTransient<INotificationHandler<EM2Devs.Todo.Application.Events.LevelUpEvent>,
    EM2Devs.Todo.Application.Events.NotificationCreationHandler>();
builder.Services.AddTransient<INotificationHandler<EM2Devs.Todo.Application.Events.StreakMilestoneReachedEvent>,
    EM2Devs.Todo.Application.Events.NotificationCreationHandler>();
builder.Services.AddTransient<INotificationHandler<EM2Devs.Todo.Application.Events.QuestCompletedEvent>,
    EM2Devs.Todo.Application.Events.NotificationCreationHandler>();

// Notification inbox handlers.
builder.Services.AddTransient<IRequestHandler<ListNotificationsQuery, Result<IReadOnlyList<Notification>>>, ListNotificationsQueryHandler>();
builder.Services.AddTransient<IRequestHandler<MarkNotificationReadCommand, Result<Notification>>, MarkNotificationReadCommandHandler>();
builder.Services.AddTransient<IRequestHandler<DismissNotificationCommand, Result<Notification>>, DismissNotificationCommandHandler>();

// FluentValidation + pipeline behavior (ADR-018)
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskCommandValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

string[] allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(CorsPolicyName, policy =>
        {
            policy.SetIsOriginAllowed(_ => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });
}
else if (allowedOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(CorsPolicyName, policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });
}

// JWT Bearer authentication (Phase 0 multi-user auth).
IConfigurationSection jwtConfig = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidAudience = jwtConfig["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtConfig["Key"]
                    ?? throw new InvalidOperationException("Jwt:Key not configured"))),
            ClockSkew = TimeSpan.Zero
        };

        // SignalR's JS client sends the access token as a query-string parameter
        // during the WebSocket/Server-Sent-Events handshake because browsers can't
        // attach custom Authorization headers on those upgrade requests. Narrow this
        // allowance to the notifications hub path only — other endpoints must keep
        // using the standard Authorization header so we don't broaden the attack surface.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                string? accessToken = context.Request.Query["access_token"];
                PathString path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken)
                    && path.StartsWithSegments("/hubs/notifications", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Intentionally no FallbackPolicy: endpoints must opt in via [Authorize] on their controller.
// This keeps method-not-allowed (405) behaviour intact for unsupported verbs like TRACE/OPTIONS,
// which would otherwise be short-circuited to 401 by the fallback before routing completes.
builder.Services.AddAuthorization();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddMvc();

builder.Services.AddControllers(options =>
    {
        options.ModelBinderProviders.Insert(0, new DateOnlyModelBinderProvider());
        options.Filters.Add<EM2Devs.Todo.Api.Middleware.RejectUnknownQueryParametersFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
    });
builder.Services.AddOpenApi();

// Real-time notification push over SignalR (feat/signalr-notifications).
builder.Services.AddSignalR();
builder.Services.AddSingleton<INotificationPublisher, SignalRNotificationPublisher>();

var app = builder.Build();

bool isNonProduction = app.Environment.IsDevelopment()
    || string.Equals(app.Environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase);
bool autoMigrateRequested = string.Equals(
    Environment.GetEnvironmentVariable("AUTO_MIGRATE"), "true", StringComparison.OrdinalIgnoreCase);

if (!string.IsNullOrEmpty(connectionString) && isNonProduction && autoMigrateRequested)
{
    await app.ApplyMigrationsAsync().ConfigureAwait(false);
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapDefaultEndpoints();
if (app.Environment.IsDevelopment() || allowedOrigins.Length > 0)
{
    app.UseCors(CorsPolicyName);
}
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");

if (app.Environment.IsDevelopment())
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        foreach (var ip in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up
                && n.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
            .SelectMany(n => n.GetIPProperties().UnicastAddresses)
            .Where(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
        {
            Console.WriteLine($"  Network: http://{ip.Address}:5001");
        }
    });
}

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program;
