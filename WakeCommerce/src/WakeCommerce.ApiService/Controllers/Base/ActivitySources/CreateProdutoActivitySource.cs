using System.Diagnostics;
using Serilog.Enrichers.Span;

namespace WakeCommerce.ApiService.Controllers.Base.ActivitySources
{
    public static class CreateProdutoActivitySource
    {
        public static string Name = "WakeCommerceActivitySource";
        public static ActivitySource instance = new ActivitySource(Name, "1.0.0");

        public static Activity StartActivity(string name)
        {
            // Verifique se a criação da Activity está acontecendo corretamente
            if (instance.HasListeners())  // Supondo que você esteja usando OpenTelemetry ou algo semelhante
            {
                return new Activity(name); // Ou alguma outra lógica para iniciar a atividade
            }

            return null; // Caso não tenha listeners ou alguma outra condição
        }
    }
}
