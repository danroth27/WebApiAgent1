using k8s.Models;

var builder = DistributedApplication.CreateBuilder(args);

var openai = builder.AddConnectionString("openai");

var apiService = builder.AddProject<Projects.WebApiAgent1>("webapiagent1")
    .WithReference(openai)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.BlazorApp1>("blazorapp1")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
