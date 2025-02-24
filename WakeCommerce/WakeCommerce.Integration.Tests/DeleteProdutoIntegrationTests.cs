using System.Net.Http.Json;
using System.Net;
using WakeCommerce.ApiService.Controllers.Base;
using WakeCommerce.Application.Queries.Response;
using System.Text.Json;

namespace WakeCommerce.Integration.Tests
{
    public class DeleteProdutoIntegrationTests
    {
        private readonly HttpClient _client;
        public DeleteProdutoIntegrationTests()
        {
            var uriBase = new Uri("https://localhost:7359");
            _client = new HttpClient();
            _client.BaseAddress = uriBase;
        }
        [Fact]
        public async Task DeleteProduto_DeveRetornarNotFound_QuandoProdutoExistir()
        {
            // Arrange - Criando um produto antes de tentar deletar
            var novoProduto = new { Nome = "Produto Teste", Descricao = "Descrição", Preco = 10.0m, Estoque = 5 };
            var response = await _client.PostAsJsonAsync("/produto/create", novoProduto);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var successResponse = JsonSerializer.Deserialize<SuccessResponse<ProdutoResponse>>(json, options);

            // Act - Tentando deletar o produto criado
            var deleteResponse = await _client.DeleteAsync($"/produto/{successResponse.Data.Id}");

            // Assert
            Assert.Equal(deleteResponse.StatusCode, HttpStatusCode.OK);
        }

        [Fact]
        public async Task DeleteProduto_DeveRetornarNotFound_QuandoProdutoNaoExistir()
        {
            // Act - Tentando deletar um produto com um ID inexistente
            var deleteResponse = await _client.DeleteAsync("/api/produto/999");

            // Assert
            Assert.Equal(deleteResponse.StatusCode, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteProduto_DeveRetornarBadRequest_QuandoIdForZero()
        {
            // Act
            var deleteResponse = await _client.DeleteAsync("/api/produto/0");

            // Assert
            Assert.Equal(deleteResponse.StatusCode, HttpStatusCode.NotFound);
        }
    }
}
