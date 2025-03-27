using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using Serilog.Context;
using WakeCommerce.ApiService.Controllers.Base.ActivitySources;
using WakeCommerce.Application.Commands;
using WakeCommerce.Application.Queries;
using WakeCommerce.Application.Queries.Request;
using WakeCommerce.Core.Mediator;
using WakeCommerce.Core.Messages.CommonMessages.Notifications;

namespace WakeCommerce.ApiService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProdutoController : Base.ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IFindProdutoQueryHandler _findProdutoQueryHandler;
        private readonly ILogger<ProdutoController> _logger;

        private readonly Tracer _tracer;
        public ProdutoController(
            INotificationHandler<DomainNotification> notifications, 
            IMediatorHandler mediatorHandler, 
            IMediator mediator, 
            IFindProdutoQueryHandler findProdutoQueryHandler, 
            ILogger<ProdutoController> logger,
            TracerProvider tracerProvider)
            : base(notifications)
        {
            _logger = logger;
            _mediator = mediator;
            _findProdutoQueryHandler = findProdutoQueryHandler;

            _tracer = tracerProvider.GetTracer("WakeCommerceActivitySource");
        }

        [HttpPost("create")]
        public async Task<IActionResult> Post([FromBody] CreateProdutoCommand command)
        {
            using (var createProdutoSpan = _tracer.StartActiveSpan(DiagnosticNames.CriarProduto))
            {
                createProdutoSpan.SetAttribute("produto.nome", command.Nome);

                using (LogContext.PushProperty("TraceId", Activity.Current?.TraceId.ToString()))
                {
                    _logger.LogInformation("Send Command");
                }

                using (var validaProdutoSpan = _tracer.StartActiveSpan(DiagnosticNames.ValidarProduto))
                {
                    if (!OperacaoValida())
                    {
                        _logger.LogInformation("Operação inválida ao criar o produto");

                        validaProdutoSpan.SetAttribute("produto.valid", "false");
                        validaProdutoSpan.AddEvent("Produto Invalido");

                        return BadRequest(ObterMensagensErro());
                    }
                    else 
                    {
                        validaProdutoSpan.SetAttribute("produto.valid", "true");
                        validaProdutoSpan.AddEvent("Produto Valido");
                    }

                }

                using (var validaProdutoSpan = _tracer.StartActiveSpan("SalvarProduto"))
                {
                    var produtoCriado = await _mediator.Send(command);

                    createProdutoSpan.AddEvent("Produto cadastrado");

                    DiagnosticsConfig.ProdutosCadastrados.Add(1);

                    return Created("by-id", produtoCriado);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromBody] UpdateProdutoCommand command, int id)
        {
            if (command.Id != id)
                return BadRequest();

            var produtoAtualizado = await _mediator.Send(command);

            if (!OperacaoValida())
            {
                return BadRequest(ObterMensagensErro());
            }

            return Ok(produtoAtualizado);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteProdutoCommand(id);

            await _mediator.Send(command);

            if (!OperacaoValida())
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpGet("by-id/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetProdutoByIdRequest(id);

            var result = await _findProdutoQueryHandler.GetProdutoById(query);

            if (result == null) return StatusCode(StatusCodes.Status404NotFound);

            return Ok(result);
        }

        [HttpPost("by-nome")]
        public async Task<IActionResult> GetByNome([FromBody] GetProdutoByNomeRequest query)
        {
            var result = await _findProdutoQueryHandler.GetProdutoByNome(query);

            if (result == null) return StatusCode(StatusCodes.Status404NotFound);

            return Ok(result);
        }

        [HttpPost("ordenar")]
        public async Task<IActionResult> OrdenarProdutosPorCategoria([FromBody] GetProdutoRequest query)
        {
            var result = await _findProdutoQueryHandler.GetProdutos(query);

            return Ok(result);
        }

        [HttpGet("erro")]
        public async Task<IActionResult> Error()
        {
            throw new Exception("Erro simulado");
        }
    }
}
