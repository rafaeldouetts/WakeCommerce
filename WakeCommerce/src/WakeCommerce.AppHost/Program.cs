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

//var grafana = builder.AddContainer("grafana", "grafana/grafana")
//    .WithBindMount("../grafana/config", "etc/grafana/dashboards", isReadOnly: true)
//    .WithBindMount("../grafana/dashboards", "var/lib/grafana/dashboards", isReadOnly: true)
//    .WithHttpEndpoint(targetPort: 3000, name: "http");

//builder.AddContainer("prometheus", "prom/prometheus")
//    .WithBindMount("../prometheus", "etc/prometheus", isReadOnly: true)
//    .WithHttpEndpoint(port: 9090, targetPort: 9090);

builder.Build().Run();
