using MediatR;
using Microsoft.AspNetCore.Mvc;
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

        public ProdutoController(
            INotificationHandler<DomainNotification> notifications, 
            IMediatorHandler mediatorHandler, 
            IMediator mediator, 
            IFindProdutoQueryHandler findProdutoQueryHandler) 
            : base(notifications, mediatorHandler)
        {
            _mediator = mediator;
            _findProdutoQueryHandler = findProdutoQueryHandler;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Post([FromBody] CreateProdutoCommand command)
        {
            var produtoCriado = await _mediator.Send(command);

            if(!OperacaoValida())
            {
                return BadRequest(ObterMensagensErro());
            }

            return Created(produtoCriado);
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
    }
}
