using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();
builder.Services.AddSingleton<IMediator, Mediator>();

builder.Services.AddTransient<IRequestHandler<CreateTaskCommand, TodoTask>, CreateTaskCommandHandler>();
builder.Services.AddTransient<IRequestHandler<UpdateTaskStatusCommand, TodoTask?>, UpdateTaskStatusCommandHandler>();
builder.Services.AddTransient<IRequestHandler<DeleteTaskCommand, bool>, DeleteTaskCommandHandler>();
builder.Services.AddTransient<IRequestHandler<GetTaskQuery, TodoTask?>, GetTaskQueryHandler>();
builder.Services.AddTransient<IRequestHandler<ListTasksQuery, IReadOnlyList<TodoTask>>, ListTasksQueryHandler>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
    });
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program;
