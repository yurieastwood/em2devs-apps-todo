using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using EM2Devs.Todo.Application.Behaviors;
using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Application.Validators;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Infrastructure.Auth;
using EM2Devs.Todo.Infrastructure.Persistence;
using EM2Devs.Todo.ServiceDefaults;
using EM2Devs.Todo.Api.Middleware;
using Asp.Versioning;
using FluentValidation;

const string CorsPolicyName = "Frontend";

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

string? connectionString = builder.Configuration.GetConnectionString("tododb");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.AddNpgsqlDbContext<TodoDbContext>("tododb");
    builder.Services.AddScoped<ITaskRepository, PostgresTaskRepository>();
}
else
{
    builder.Services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();
}

builder.Services.AddSingleton<IPlayerProfileRepository, InMemoryPlayerProfileRepository>();

builder.Services.AddScoped<DemoCurrentUser>();
builder.Services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<DemoCurrentUser>());

builder.Services.AddScoped<IMediator, Mediator>();

// CQRS handlers (return Result<T> per ADR-018)
builder.Services.AddTransient<IRequestHandler<CreateTaskCommand, Result<TodoTask>>, CreateTaskCommandHandler>();
builder.Services.AddTransient<IRequestHandler<UpdateTaskStatusCommand, Result<TodoTask>>, UpdateTaskStatusCommandHandler>();
builder.Services.AddTransient<IRequestHandler<DeleteTaskCommand, Result<bool>>, DeleteTaskCommandHandler>();
builder.Services.AddTransient<IRequestHandler<GetTaskQuery, Result<TodoTask>>, GetTaskQueryHandler>();
builder.Services.AddTransient<IRequestHandler<ListTasksQuery, Result<IReadOnlyList<TodoTask>>>, ListTasksQueryHandler>();

builder.Services.AddTransient<INotificationHandler<EM2Devs.Todo.Application.Events.TaskCompletedEvent>,
    EM2Devs.Todo.Application.Events.XpAwardHandler>();

// FluentValidation + pipeline behavior (ADR-018)
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskCommandValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

string[] allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

if (allowedOrigins.Length > 0)
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

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddMvc();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
    });
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<DemoAuthMiddleware>();
}
app.MapDefaultEndpoints();
if (allowedOrigins.Length > 0)
{
    app.UseCors(CorsPolicyName);
}
app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program;
