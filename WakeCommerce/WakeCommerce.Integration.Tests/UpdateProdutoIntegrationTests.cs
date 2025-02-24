using System.Net.Http.Json;
using System.Net;
using WakeCommerce.ApiService.Controllers.Base;
using WakeCommerce.Application.Commands;
using WakeCommerce.Application.Queries.Response;
using System.Text.Json;

namespace WakeCommerce.Integration.Tests
{
    public class UpdateProdutoIntegrationTests
    {
        private readonly HttpClient _client;

        public UpdateProdutoIntegrationTests()
        {
            var uriBase = new Uri("https://localhost:7359");
            _client = new HttpClient();
            _client.BaseAddress = uriBase;
        }

        [Fact]
        public async Task AtualizarProduto_DeveRetornar200Ok_QuandoDadosValidos()
        {
            // Arrange

            var novoProduto = new { Nome = "Produto Teste", Descricao = "Descrição", Preco = 10.0m, Estoque = 5 };
            var response = await _client.PostAsJsonAsync("/produto/create", novoProduto);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var successResponse = JsonSerializer.Deserialize<SuccessResponse<ProdutoResponse>>(json, options);

            var produtoAtualizado = new UpdateProdutoCommand
            {
                Id = successResponse.Data.Id,
                Nome = "Produto Atualizado",
                Descricao = "Descrição atualizada",
                Preco = 79.99m,
                Estoque = 15
            };

            // Act
            response = await _client.PutAsJsonAsync($"/produto/{successResponse.Data.Id}", produtoAtualizado);
            var result = await response.Content.ReadFromJsonAsync<SuccessResponse<ProdutoResponse>>();

            // Assert
            Assert.Equal(response.StatusCode, HttpStatusCode.OK);
            Assert.NotNull(result);
            Assert.Equal(result.Data.Nome, produtoAtualizado.Nome);
            Assert.Equal(result.Data.Preco, produtoAtualizado.Preco);
        }

        [Fact]
        public async Task AtualizarProduto_DeveRetornar400BadRequest_QuandoNomeVazio()
        {
            // Arrange
            var produtoId = 1;
            var produtoAtualizado = new UpdateProdutoCommand
            {
                Id = produtoId,
                Nome = "", // Nome inválido
                Descricao = "Descrição do produto",
                Preco = 50.99m,
                Estoque = 10
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/produto/{produtoId}", produtoAtualizado);
            var result = await response.Content.ReadFromJsonAsync<ProblemDatails<List<string>>>();

            // Assert
            Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
            Assert.Contains("O nome não pode ser vazio", result.Detail);
        }

        [Fact]
        public async Task AtualizarProduto_DeveRetornar400BadRequest_QuandoPrecoMenorOuIgualAZero()
        {
            // Arrange
            var produtoId = 1;
            var produtoAtualizado = new UpdateProdutoCommand
            {
                Id = produtoId,
                Nome = "Produto Inválido",
                Descricao = "Descrição do produto",
                Preco = 0, // Preço inválido
                Estoque = 10
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/produto/{produtoId}", produtoAtualizado);
            var result = await response.Content.ReadFromJsonAsync<ProblemDatails<List<string>>>();

            // Assert
            Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
            Assert.Contains("O preço não pode ser menor que 0", result.Detail);
        }

        [Fact]
        public async Task AtualizarProduto_DeveRetornar400NotFound_QuandoProdutoNaoExiste()
        {
            // Arrange
            var produtoId = 9999; // ID que não existe no banco
            var produtoAtualizado = new UpdateProdutoCommand
            {
                Id = produtoId,
                Nome = "Produto Inexistente",
                Descricao = "Descrição",
                Preco = 100.00m,
                Estoque = 10
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/produto/{produtoId}", produtoAtualizado);

            // Assert
            Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
        }
    }
}
