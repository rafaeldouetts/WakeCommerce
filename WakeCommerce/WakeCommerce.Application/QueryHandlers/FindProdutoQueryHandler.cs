using System.Text.Json;
using WakeCommerce.Application.Queries;
using WakeCommerce.Application.Queries.Request;
using WakeCommerce.Application.Queries.Response;
using WakeCommerce.Domain.Entities;
using WakeCommerce.Domain.Repositories;
using WakeCommerce.Domain.Repository;

namespace WakeCommerce.Application.QueryHandlers
{
    public class FindProdutoQueryHandler : IFindProdutoQueryHandler
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IRedisRepository _redisRepository;

        public FindProdutoQueryHandler(IProdutoRepository produtoRepository, IRedisRepository redisRepository)
        {
            _produtoRepository = produtoRepository;
            _redisRepository = redisRepository;
        }

        public async Task<ProdutoResponse> GetProdutoById(GetProdutoByIdRequest request)
        {
            var produtoCache = await _redisRepository.ObterProdutoDoRedisAsync(request.Id.ToString());

            if(!string.IsNullOrEmpty(produtoCache))
            {
                var result = JsonSerializer.Deserialize<ProdutoResponse>(produtoCache);

                return result;
            }

            var produto = await _produtoRepository.GetById(request.Id);

            if (produto == null) return null;

            var response = new ProdutoResponse()
            {
                Id = produto.Id,
                Descricao = produto.Descricao,
                Estoque = produto.Estoque,
                Nome = produto.Nome,
                Preco = produto.Preco
            };

            return response;
        }

        public async Task<ProdutoResponse> GetProdutoByNome(GetProdutoByNomeRequest request)
        {
            var produtoCache = await _redisRepository.ObterProdutoDoRedisAsync(request.Nome);

            if (!string.IsNullOrEmpty(produtoCache))
            {
                var result = JsonSerializer.Deserialize<ProdutoResponse>(produtoCache);

                return result;
            }

            var produto = await _produtoRepository.GetByNome(request.Nome);

            if (produto == null) return null;

            var response = new ProdutoResponse() 
            { 
                Id = produto.Id,
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
