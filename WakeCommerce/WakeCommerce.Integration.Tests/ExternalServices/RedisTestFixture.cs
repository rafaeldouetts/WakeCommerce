using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using WakeCommerce.NIntegration.Tests.Fixtures.Base;

namespace WakeCommerce.NIntegration.Tests.ExternalServices
{
    public sealed class RedisTestFixture : TestFixtureBase
    {
        public Guid CreatedTenantId { get; } = Guid.NewGuid();
        public IDistributedCache DistributedCache { get; }

        public RedisTestFixture()
        {
            DistributedCache = Factory.Services.GetRequiredService<IDistributedCache>();
        }
    }
}
