using System.Net.Http.Json;
using System.Net;
using WakeCommerce.ApiService.Controllers.Base;
using WakeCommerce.Application.Commands;
using WakeCommerce.Application.Queries.Response;

namespace WakeCommerce.Integration.Tests
{
    public class CreateProdutoIntegrationTests
    {
        private readonly HttpClient _client;

        public CreateProdutoIntegrationTests()
        {
            var uriBase = new Uri("https://localhost:7359");
            _client = new HttpClient();
            _client.BaseAddress = uriBase;
        }

        [Fact]
        public async Task CriarProduto_DeveRetornar201Created_QuandoDadosValidos()
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
            var response = await _client.PostAsJsonAsync("/produto/create", produto);
            var result = await response.Content.ReadFromJsonAsync<SuccessResponse<ProdutoResponse>>();

            // Assert
            Assert.Equal(response.StatusCode, HttpStatusCode.Created);
            Assert.NotNull(result);
            Assert.Equal(result.Data.Nome, produto.Nome);
            Assert.Equal(result.Data.Preco, produto.Preco);
        }

        [Fact]
        public async Task CriarProduto_DeveRetornar400BadRequest_QuandoNomeVazio()
        {
            // Arrange
            var produto = new CreateProdutoCommand
            {
                Nome = "", // Nome inválido
                Descricao = "Descrição do produto",
                Preco = 50.99m,
                Estoque = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/produto/create", produto);
            var result = await response.Content.ReadFromJsonAsync<ProblemDatails<List<string>>>();

            // Assert
            Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
            Assert.Contains("O nome não pode ser vazio", result.Detail);
        }

        [Fact]
        public async Task CriarProduto_DeveRetornar400BadRequest_QuandoPrecoMenorOuIgualAZero()
        {
            // Arrange
            var produto = new CreateProdutoCommand
            {
                Nome = "Produto Inválido",
                Descricao = "Descrição do produto",
                Preco = 0, // Preço inválido
                Estoque = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/produto/create", produto);
            var result = await response.Content.ReadFromJsonAsync<ProblemDatails<List<string>>>();

            // Assert
            Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
            Assert.Contains("O preço não pode ser menor que 0", result.Detail);
        }
    }
}
