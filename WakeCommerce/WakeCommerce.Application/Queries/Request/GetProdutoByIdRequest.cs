namespace WakeCommerce.Application.Queries.Request
{
    public class GetProdutoByIdRequest
    {
        public GetProdutoByIdRequest(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }
}
