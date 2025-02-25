namespace WakeCommerce.Application.Queries.Request
{
    public class GetProdutoRequest
    {
        public string OrderBy { get; set; } = "nome";
        public bool Descending { get; set; } = false;
    }
}
