var builder = DistributedApplication.CreateBuilder(args);

//pacote: Aspire.Hosting.Redis
var cache = builder.AddRedis("cache");

//pacote: Aspire.Hosting.SqlServer
var sql = builder.AddSqlServer("sql");

var apiService = builder.AddProject<Projects.WakeCommerce_ApiService>("apiservice");

builder.Build().Run();
