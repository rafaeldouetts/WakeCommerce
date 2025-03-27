
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace WakeCommerce.ApiService.Controllers.Base.ActivitySources
{
    public static class DiagnosticsConfig
    {
        public const string ServiceName = "WakeCommerceActivitySource";

        public static Meter Meter = new(ServiceName);
        public static ActivitySource Source = new(ServiceName);

        public static Counter<long> ProdutosCadastrados = Meter.CreateCounter<long>("Produtos.count");
    }
}
