var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .AddDatabase("tododb");

var redis = builder.AddRedis("redis");

builder.AddProject<Projects.EM2Devs_Todo_Api>("api")
    .WithHttpEndpoint(port: 5001, name: "http")
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis);

builder.Build().Run();
