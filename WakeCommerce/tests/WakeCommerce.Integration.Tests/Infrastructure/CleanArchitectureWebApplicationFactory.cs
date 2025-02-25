using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Testcontainers.Redis;

namespace WakeCommerce.NIntegration.Tests.Infrastructure
{
    public sealed class CleanArchitectureWebApplicationFactory : WebApplicationFactory<Program>
    {
        public delegate void RegisterCustomServicesHandler(
            IServiceCollection services);

        private readonly bool _addTestAuthentication;
        private readonly RegisterCustomServicesHandler? _registerCustomServicesHandler;

        public CleanArchitectureWebApplicationFactory(
            RegisterCustomServicesHandler? registerCustomServicesHandler,
            bool addTestAuthentication)
        {
            _registerCustomServicesHandler = registerCustomServicesHandler;
            _addTestAuthentication = addTestAuthentication;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            

            var redisPort = GlobalSetupFixture.RedisContainer.GetMappedPublicPort(RedisBuilder.RedisPort);

            Environment.SetEnvironmentVariable("ConnectionStrings:DefaultConnection",
                GlobalSetupFixture.DatabaseConnectionString);

            Environment.SetEnvironmentVariable("ConnectionStrings:RedisConnection",
                $"localhost:{redisPort}");

            builder.ConfigureServices(services =>
            {
                //if (_addTestAuthentication)
                //{
                //    services.AddAuthentication(options =>
                //    {
                //        options.DefaultAuthenticateScheme = "Testing";
                //        options.DefaultChallengeScheme = "Testing";
                //    }).AddTestAuthentication(_ => { });
                //}

                _registerCustomServicesHandler?.Invoke(services);
            });

            base.ConfigureWebHost(builder);
        }
    }
}