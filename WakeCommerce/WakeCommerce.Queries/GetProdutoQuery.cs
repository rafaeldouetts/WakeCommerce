
namespace WakeCommerce.Queries
{
    public class GetProdutoQuery
    {
        public string? Categoria { get; set; }
        public string OrderBy { get; set; } = "nome";
        public bool Descending { get; set; } = false;
    }
}
