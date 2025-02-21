var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.WakeCommerce_ApiService>("apiservice");

builder.Build().Run();
