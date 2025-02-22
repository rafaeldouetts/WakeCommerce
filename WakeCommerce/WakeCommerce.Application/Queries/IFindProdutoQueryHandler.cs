using WakeCommerce.Application.Queries.Request;
using WakeCommerce.Application.Queries.Response;

namespace WakeCommerce.Application.Queries
{
    public interface IFindProdutoQueryHandler
    {
        Task<ProdutoResponse> GetProdutoByNome(GetProdutoByNomeRequest request);
        Task<ProdutoResponse> GetProdutoById(GetProdutoByIdRequest request);
        Task<IEnumerable<ProdutoResponse>> GetProdutos(GetProdutoRequest request);
    }
}
