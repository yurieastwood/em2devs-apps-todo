var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .AddDatabase("tododb");

var redis = builder.AddRedis("redis");

var api = builder.AddProject<Projects.EM2Devs_Todo_Api>("api")
    .WithHttpEndpoint(port: 5001, name: "http", isProxied: false)
    .WithEnvironment("ASPNETCORE_URLS", "http://0.0.0.0:5001")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithEnvironment("AUTO_MIGRATE", "true")
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis);

builder.AddProject<Projects.EM2Devs_Todo_Worker>("worker")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis)
    .WaitFor(api);

builder.AddNpmApp("web", "../EM2Devs.Todo.Web", "dev")
    .WithNpmPackageInstallation()
    .WithHttpEndpoint(port: 5173, env: "PORT", isProxied: false)
    .WithEnvironment("NODE_ENV", builder.Environment.EnvironmentName.ToLowerInvariant())
    .WithEnvironment("API_BASE_URL", api.GetEndpoint("http"))
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
