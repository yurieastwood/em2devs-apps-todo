using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Infrastructure.Persistence;
using EM2Devs.Todo.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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

builder.Services.AddSingleton<IMediator, Mediator>();

builder.Services.AddTransient<IRequestHandler<CreateTaskCommand, TodoTask>, CreateTaskCommandHandler>();
builder.Services.AddTransient<IRequestHandler<UpdateTaskStatusCommand, TodoTask?>, UpdateTaskStatusCommandHandler>();
builder.Services.AddTransient<IRequestHandler<DeleteTaskCommand, bool>, DeleteTaskCommandHandler>();
builder.Services.AddTransient<IRequestHandler<GetTaskQuery, TodoTask?>, GetTaskQueryHandler>();
builder.Services.AddTransient<IRequestHandler<ListTasksQuery, IReadOnlyList<TodoTask>>, ListTasksQueryHandler>();

builder.Services.AddTransient<INotificationHandler<EM2Devs.Todo.Application.Events.TaskCompletedEvent>,
    EM2Devs.Todo.Application.Events.XpAwardHandler>();

builder.Services.AddCors(options =>
{
    string[] allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
    });
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseCors("Frontend");
app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program;
