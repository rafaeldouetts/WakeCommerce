using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WakeCommerce.NIntegration.Tests.Infrastructure;

namespace WakeCommerce.NIntegration.Tests.Fixtures.Base
{
    public class TestFixtureBase
    {
        public HttpClient ServerClient { get; }
        protected CleanArchitectureWebApplicationFactory Factory { get; }

        public TestFixtureBase(bool useTestAuthentication = true)
        {
            Factory = new CleanArchitectureWebApplicationFactory(
                RegisterCustomServicesHandler,
                useTestAuthentication);

            ServerClient = Factory.CreateClient();
            ServerClient.Timeout = TimeSpan.FromMinutes(5);
        }

        protected virtual void RegisterCustomServicesHandler(
            IServiceCollection services)
        {
        }
    }
}
