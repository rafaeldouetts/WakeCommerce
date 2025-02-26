using System.Net.Http.Json;
using OpenTelemetry.Trace;
using WakeCommerce.ApiService.Controllers.Base;
using WakeCommerce.Application.Commands;

namespace WakeCommerce.BDD.Tests
{
    [Binding]
    public class CriarProdutoStepDefinitions
    {
        private HttpClient _client;
        CreateProdutoCommand _produto;
        HttpResponseMessage _response;
        ProblemDatails<List<string>> _problemDatails;


        public CriarProdutoStepDefinitions()
        {
            _client = new HttpClient();
            var uriBase = new Uri("http://localhost:5001/");
            _client.BaseAddress = uriBase;
        }

        [When("eu envio uma solicitação para criar o produto")]
        public async Task WhenEuEnvioUmaSolicitacaoParaCriarOProduto()
        {
            _response = await _client.PostAsJsonAsync("/produto/create", _produto);
            _problemDatails = await _response.Content.ReadFromJsonAsync<ProblemDatails<List<string>>>();
        }

        [Given("que eu tenho um produto com o preco negativo")]
        public void GivenQueEuTenhoUmProdutoComOPrecoNegativo()
        {
            _produto = new CreateProdutoCommand
            {
                Nome = "Produto Teste",
                Descricao = "Descrição do produto",
                Preco = -50.99m,
                Estoque = 10
            };
        }

        [Then("eu devo receber um status de resposta {int}")]
        public void ThenEuDevoReceberUmStatusDeResposta(int statusCode)
        {
            Assert.Equal((int)_response.StatusCode, statusCode);
        }

        [Then("eu devo receber a mesangem {string}")]
        public void ThenEuDevoReceberAMesangem(string mensagem)
        {
            Assert.True(_problemDatails.Detail.Contains(mensagem));
        }
    }
}
