var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .AddDatabase("tododb");

var redis = builder.AddRedis("redis");

var api = builder.AddProject<Projects.EM2Devs_Todo_Api>("api")
    .WithHttpEndpoint(port: 5001, name: "http")
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis);

builder.AddNpmApp("web", "../EM2Devs.Todo.Web", "dev")
    .WithHttpEndpoint(port: 5173, env: "PORT")
    .WithEnvironment("API_BASE_URL", api.GetEndpoint("http"))
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
