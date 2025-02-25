using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json.Linq;
using WakeCommerce.NIntegration.Tests.Fixtures;

namespace WakeCommerce.NIntegration.Tests.UtilityTests
{
    internal class HealthChecksTests
    {
        private readonly ProdutoTestFixture _fixture = new();

        [Test, Order(0)]
        public async Task Should_Return_Healthy()
        {
            var response = await _fixture.ServerClient.GetAsync("/healthz");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();

            var json = JObject.Parse(content);
            json["status"]!.Value<string>().Should().Be(HealthStatus.Healthy.ToString());
        }
    }
}
