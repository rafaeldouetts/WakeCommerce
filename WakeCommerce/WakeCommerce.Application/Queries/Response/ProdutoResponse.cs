
namespace WakeCommerce.Application.Queries.Response
{
    public class ProdutoResponse
    {
        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public int Estoque { get; set; }
    }
}
