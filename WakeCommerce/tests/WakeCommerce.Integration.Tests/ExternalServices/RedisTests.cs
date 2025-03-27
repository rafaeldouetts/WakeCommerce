using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using WakeCommerce.ApiService.Controllers.Base;
using WakeCommerce.Application.Commands;
using WakeCommerce.Application.Queries.Response;
using WakeCommerce.NIntegration.Tests.Extensions;
using WakeCommerce.NIntegration.Tests.Fixtures;

namespace WakeCommerce.NIntegration.Tests.ExternalServices
{
    public sealed class RedisTests
    {
        private readonly ProdutoTestFixture _fixture = new(); 
        private readonly RedisTestFixture _redisFixture = new();

        [Test, Order(1)]
        public async Task BuscarProdutoPorNomeRedis_AposSerCadastrado_DeveRetornarProduto()
        {
            // Arrange
            var produto = new CreateProdutoCommand
            {
                Nome = "Produto Teste",
                Descricao = "Descrição do produto",
                Preco = 50.99m,
                Estoque = 10
            };

            // Act
            var response = await _fixture.ServerClient.PostAsJsonAsync("/produto/create", produto);
            var result = await response.Content.ReadFromJsonAsync<SuccessResponse<ProdutoResponse>>();

            var produtoRedis = await _redisFixture.DistributedCache.GetStringAsync(produto.Nome);

            // Assert
            produtoRedis.Should().NotBeNull();
        }

        [Test, Order(2)]
        public async Task BuscarProdutoPorIdRedis_AposSerCadastrado_DeveRetornarProduto()
        {
            // Arrange
            var produto = new CreateProdutoCommand
            {
                Nome = "Produto Teste",
                Descricao = "Descrição do produto",
                Preco = 50.99m,
                Estoque = 10
            };

            // Act
            var response = await _fixture.ServerClient.PostAsJsonAsync("/produto/create", produto);
            var result = await response.Content.ReadFromJsonAsync<SuccessResponse<ProdutoResponse>>();

            var json = await _redisFixture.DistributedCache.GetStringAsync(result.Data.Id.ToString());

            // Assert
            json.Should().NotBeNull();
        }
    }
}
