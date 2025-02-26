var builder = DistributedApplication.CreateBuilder(args);

//pacote: Aspire.Hosting.Redis
var cache = builder.AddRedis("cache").WithRedisInsight();

//pacote: Aspire.Hosting.SqlServer
var sql = builder.AddSqlServer("sql")
    .WaitFor(cache);

var apiService = builder.AddProject<Projects.WakeCommerce_ApiService>("apiservice")
    .WithOtlpExporter()
    .WithReference(sql)
    .WithReference(cache)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Aspire")
    .WaitFor(sql);

builder.Build().Run();
