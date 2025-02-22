using WakeCommerce.Application.Queries;
using WakeCommerce.Application.Queries.Request;
using WakeCommerce.Application.Queries.Response;
using WakeCommerce.Domain.Entities;
using WakeCommerce.Domain.Repositories;

namespace WakeCommerce.Application.QueryHandlers
{
    public class FindProdutoQueryHandler : IFindProdutoQueryHandler
    {
        private readonly IProdutoRepository _produtoRepository;

        public FindProdutoQueryHandler(IProdutoRepository produtoRepository)
        {
            _produtoRepository = produtoRepository;
        }

        public async Task<ProdutoResponse> GetProdutoById(GetProdutoByIdRequest request)
        {
            var produto = await _produtoRepository.GetById(request.Id);

            var response = new ProdutoResponse()
            {
                Descricao = produto.Descricao,
                Estoque = produto.Estoque,
                Nome = produto.Nome,
                Preco = produto.Preco
            };

            return response;
        }

        public async Task<ProdutoResponse> GetProdutoByNome(GetProdutoByNomeRequest request)
        {
            var produto = await _produtoRepository.GetByNome(request.Nome);

            var response = new ProdutoResponse() 
            { 
                Descricao = produto.Descricao,
                Estoque = produto.Estoque,
                Nome = produto.Nome,
                Preco = produto.Preco
            };

            return response;
        }

        public async Task<IEnumerable<ProdutoResponse>> GetProdutos(GetProdutoRequest request)
        {
            List<Produto> produtos = await _produtoRepository.GetAll(request.Descending, request.OrderBy);

            var result = produtos.Select(produto => new ProdutoResponse()
            {
                Descricao = produto.Descricao,
                Estoque = produto.Estoque,
                Nome = produto.Nome,
                Preco = produto.Preco
            });

            return result;
        }
    }
}
