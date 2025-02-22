var builder = DistributedApplication.CreateBuilder(args);

//pacote: Aspire.Hosting.Redis
var cache = builder.AddRedis("cache");

//pacote: Aspire.Hosting.SqlServer
var sql = builder.AddSqlServer("sql").WaitFor(cache);

var apiService = builder.AddProject<Projects.WakeCommerce_ApiService>("apiservice").WaitFor(sql);

builder.Build().Run();
