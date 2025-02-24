using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Azure;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using WakeCommerce.ApiService.Controllers.Base;
using WakeCommerce.Application.Commands;
using WakeCommerce.Application.Queries.Request;
using WakeCommerce.Application.Queries.Response;
using WakeCommerce.Domain.Entities;
using WakeCommerce.NIntegration.Tests.ExternalServices;
using WakeCommerce.NIntegration.Tests.Fixtures;

namespace WakeCommerce.NIntegration.Tests.Controller
{
    public sealed class ProdutoControllerTests
    {
        private readonly ProdutoTestFixture _fixture = new();
        private readonly RedisTestFixture _redisFixture = new();

        [Test, Order(1)]
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
            var response = await _fixture.ServerClient.PostAsJsonAsync("/produto/create", produto);
            var result = await response.Content.ReadFromJsonAsync<SuccessResponse<ProdutoResponse>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            result.Should().NotBeNull();
            result.Data.Nome.Should().Be(produto.Nome);
            result.Data.Preco.Should().Be(produto.Preco);
        }
        [Test, Order(2)]
        public async Task BuscarProdutosPreCarregados_DeveRetornar200Ok_QuandoDadosValidos()
        {
            // Arrange
            var request = new GetProdutoRequest();

            // Act
            var response = await _fixture.ServerClient.PostAsJsonAsync($"/produto/ordenar/", request);

            var result = await response.Content.ReadFromJsonAsync<SuccessResponse<IEnumerable<ProdutoResponse>>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result!.Data.Should().NotBeNull();
            result!.Data.Should().NotBeEmpty();
        }

        [Test, Order(3)]
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
            var response = await _fixture.ServerClient.PostAsJsonAsync("/produto/create", produto);
            var result = await response.Content.ReadFromJsonAsync<ProblemDatails<List<string>>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.Detail.Should().Contain("O nome não pode ser vazio");
        }

        [Test, Order(4)]
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
            var response = await _fixture.ServerClient.PostAsJsonAsync("/produto/create", produto);
            var result = await response.Content.ReadFromJsonAsync<ProblemDatails<List<string>>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.Detail.Should().Contain("O preço não pode ser menor que 0");
        }

        //Delete
        [Test, Order(5)]
        public async Task DeleteProduto_DeveRetornarNotFound_QuandoProdutoExistir()
        {
            // Arrange - Criando um produto antes de tentar deletar
            var novoProduto = new { Nome = "Produto Teste", Descricao = "Descrição", Preco = 10.0m, Estoque = 5 };
            var response = await _fixture.ServerClient.PostAsJsonAsync("/produto/create", novoProduto);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var successResponse = JsonSerializer.Deserialize<SuccessResponse<ProdutoResponse>>(json, options);

            // Act - Tentando deletar o produto criado
            response = await _fixture.ServerClient.DeleteAsync($"/produto/{successResponse.Data.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test, Order(6)]
        public async Task DeleteProduto_DeveRetornarNotFound_QuandoProdutoNaoExistir()
        {
            // Act - Tentando deletar um produto com um ID inexistente
            var deleteResponse = await _fixture.ServerClient.DeleteAsync("/produto/99999");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test, Order(7)]
        public async Task DeleteProduto_DeveRetornarBadRequest_QuandoIdForZero()
        {
            // Act
            var deleteResponse = await _fixture.ServerClient.DeleteAsync("/produto/0");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        //update

        [Test, Order(8)]
        public async Task AtualizarProduto_DeveRetornar200Ok_QuandoDadosValidos()
        {
            // Arrange

            var novoProduto = new { Nome = "Produto Teste", Descricao = "Descrição", Preco = 10.0m, Estoque = 5 };
            var response = await _fixture.ServerClient.PostAsJsonAsync("/produto/create", novoProduto);
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
            response = await _fixture.ServerClient.PutAsJsonAsync($"/produto/{successResponse.Data.Id}", produtoAtualizado);
            var result = await response.Content.ReadFromJsonAsync<SuccessResponse<ProdutoResponse>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Data.Nome.Should().Be(produtoAtualizado.Nome);
            result.Data.Preco.Should().Be(produtoAtualizado.Preco);
        }

        [Test, Order(9)]
        public async Task AtualizarProduto_DeveRetornar400Badrequest_QuandoIdDivergente()
        {
            // Arrange

            var novoProduto = new { Nome = "Produto Teste", Descricao = "Descrição", Preco = 10.0m, Estoque = 5 };
            var response = await _fixture.ServerClient.PostAsJsonAsync("/produto/create", novoProduto);
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
            response = await _fixture.ServerClient.PutAsJsonAsync($"/produto/{successResponse.Data.Id + 1}", produtoAtualizado);
            var result = await response.Content.ReadFromJsonAsync<SuccessResponse<ProdutoResponse>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test, Order(10)]
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
            var response = await _fixture.ServerClient.PutAsJsonAsync($"/produto/{produtoId}", produtoAtualizado);
            var result = await response.Content.ReadFromJsonAsync<ProblemDatails<List<string>>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.Detail.Should().Contain("O nome não pode ser vazio");
        }

        [Test, Order(11)]
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
            var response = await _fixture.ServerClient.PutAsJsonAsync($"/produto/{produtoId}", produtoAtualizado);
            var result = await response.Content.ReadFromJsonAsync<ProblemDatails<List<string>>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.Detail.Should().Contain("O preço não pode ser menor que 0");
        }

        [Test, Order(12)]
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
            var response = await _fixture.ServerClient.PutAsJsonAsync($"/produto/{produtoId}", produtoAtualizado);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        //Get - by-id
        [Test, Order(13)]
        public async Task BuscarProdutoPorId_DeveRetornar404NotFound_QuandoNaoCadastrado()
        {
            // Act
            var response = await _fixture.ServerClient.GetAsync($"/produto/by-id/{int.MaxValue}");
            var result = await response.Content.ReadFromJsonAsync<SuccessResponse<ProdutoResponse>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test, Order(14)]
        public async Task BuscarProdutoPordId_DeveRetornar200Ok_QuandoDadosValidos()
        {
            // Arrange

            var novoProduto = new { Nome = "Produto Teste", Descricao = "Descrição", Preco = 10.0m, Estoque = 5 };
            var response = await _fixture.ServerClient.PostAsJsonAsync("/produto/create", novoProduto);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var successResponse = JsonSerializer.Deserialize<SuccessResponse<ProdutoResponse>>(json, options);

            // Act
            response = await _fixture.ServerClient.GetAsync($"/produto/by-id/{successResponse.Data.Id}");
            var result = await response.Content.ReadFromJsonAsync<SuccessResponse<ProdutoResponse>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Data.Nome.Should().Be(novoProduto.Nome);
            result.Data.Preco.Should().Be(novoProduto.Preco);
        }

        [Test, Order(15)]
        public async Task BuscarProdutoPordIdForaDoRedis_DeveRetornar200Ok_QuandoDadosValidos()
        {
            // Arrange

            var novoProduto = new { Nome = "Produto Teste", Descricao = "Descrição", Preco = 10.0m, Estoque = 5 };
            var response = await _fixture.ServerClient.PostAsJsonAsync("/produto/create", novoProduto);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var successResponse = JsonSerializer.Deserialize<SuccessResponse<ProdutoResponse>>(json, options);

            await _redisFixture.DistributedCache.RemoveAsync(successResponse.Data.Id.ToString());

            // Act
            response = await _fixture.ServerClient.GetAsync($"/produto/by-id/{successResponse.Data.Id}");
            var result = await response.Content.ReadFromJsonAsync<SuccessResponse<ProdutoResponse>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Data.Nome.Should().Be(novoProduto.Nome);
            result.Data.Preco.Should().Be(novoProduto.Preco);
        }

        //Get - by-nome
        [Test, Order(16)]
        public async Task BuscarProdutoPorNome_DeveRetornar404NotFound_QuandoNaoCadastrado()
        {
            // Arrange
            var request = new GetProdutoByNomeRequest()
            {
                Nome = "Produto não cadastrado"
            };

            // Act
            var response = await _fixture.ServerClient.PostAsJsonAsync($"/produto/by-nome/", request);
            var result = await response.Content.ReadFromJsonAsync<SuccessResponse<ProdutoResponse>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test, Order(17)]
        public async Task BuscarProdutoPorNome_DeveRetornar200Ok_QuandoDadosValidos()
        {
            // Arrange

            var novoProduto = new { Nome = "Produto Por Nome", Descricao = "Descrição", Preco = 10.0m, Estoque = 5 };
            var response = await _fixture.ServerClient.PostAsJsonAsync("/produto/create", novoProduto);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var successResponse = JsonSerializer.Deserialize<SuccessResponse<ProdutoResponse>>(json, options);

            var request = new GetProdutoByNomeRequest()
            {
                Nome = novoProduto.Nome
            };

            // Act
            response = await _fixture.ServerClient.PostAsJsonAsync($"/produto/by-nome/", request);
            var result = await response.Content.ReadFromJsonAsync<SuccessResponse<ProdutoResponse>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Data.Nome.Should().Be(novoProduto.Nome);
            result.Data.Preco.Should().Be(novoProduto.Preco);
        }

        [Test, Order(18)]
        public async Task BuscarProdutoPorNomeForaDoRedis_DeveRetornar200Ok_QuandoDadosValidos()
        {
            // Arrange

            var novoProduto = new { Nome = "Produto Por Nome", Descricao = "Descrição", Preco = 10.0m, Estoque = 5 };
            var response = await _fixture.ServerClient.PostAsJsonAsync("/produto/create", novoProduto);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var successResponse = JsonSerializer.Deserialize<SuccessResponse<ProdutoResponse>>(json, options);

            var request = new GetProdutoByNomeRequest()
            {
                Nome = novoProduto.Nome
            };

            await _redisFixture.DistributedCache.RemoveAsync(novoProduto.Nome);

            // Act
            response = await _fixture.ServerClient.PostAsJsonAsync($"/produto/by-nome/", request);
            var result = await response.Content.ReadFromJsonAsync<SuccessResponse<ProdutoResponse>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Data.Nome.Should().Be(novoProduto.Nome);
            result.Data.Preco.Should().Be(novoProduto.Preco);
        }

        //Get - order
        [Test, Order(19)]
        public async Task BuscarProdutoOrdenado_DeveRetornar200Ok_QuandoDadosValidos()
        {
            // Arrange

            var novoProduto = new { Nome = "Produto Ordenado", Descricao = "Descrição", Preco = 777.0m, Estoque = 7 };
            var response = await _fixture.ServerClient.PostAsJsonAsync("/produto/create", novoProduto);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var successResponse = JsonSerializer.Deserialize<SuccessResponse<ProdutoResponse>>(json, options);

            var request = new GetProdutoRequest();

            // Act
            response = await _fixture.ServerClient.PostAsJsonAsync($"/produto/ordenar/", request);

            var result = await response.Content.ReadFromJsonAsync<SuccessResponse<IEnumerable<ProdutoResponse>>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result!.Data.Should().NotBeNull();
            result!.Data.Should().NotBeEmpty();
            result!.Data.Should().ContainSingle(p => p.Nome == novoProduto.Nome && p.Preco == novoProduto.Preco);
        }
    }
}
