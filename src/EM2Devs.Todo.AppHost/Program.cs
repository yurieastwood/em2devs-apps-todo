var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .AddDatabase("tododb");

var redis = builder.AddRedis("redis");

var api = builder.AddProject<Projects.EM2Devs_Todo_Api>("api")
    .WithHttpEndpoint(port: 5001, name: "http")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithEnvironment("AUTO_MIGRATE", "true")
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis);

builder.AddNpmApp("web", "../EM2Devs.Todo.Web", "dev")
    .WithNpmPackageInstallation()
    .WithHttpEndpoint(port: 5173, env: "PORT")
    .WithEnvironment("NODE_ENV", builder.Environment.EnvironmentName.ToLowerInvariant())
    .WithEnvironment("API_BASE_URL", api.GetEndpoint("http"))
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
