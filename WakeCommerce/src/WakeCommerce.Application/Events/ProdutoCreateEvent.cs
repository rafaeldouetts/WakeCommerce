using WakeCommerce.Application.Queries.Response;
using WakeCommerce.Core.Messages;

namespace WakeCommerce.Application.Events
{
    public class ProdutoCreateEvent : Event
    {
        public ProdutoCreateEvent(ProdutoResponse produto)
        {
            Produto = produto;
        }

        public ProdutoResponse Produto { get; set; }
    }
}
