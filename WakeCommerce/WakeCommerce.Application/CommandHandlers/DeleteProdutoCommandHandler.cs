

using MediatR;
using WakeCommerce.Application.Commands;
using WakeCommerce.Core.Mediator;
using WakeCommerce.Core.Messages;
using WakeCommerce.Core.Messages.CommonMessages.Notifications;
using WakeCommerce.Domain.Repositories;

namespace WakeCommerce.Application.CommandHandlers
{
    public class DeleteProdutoCommandHandler : IRequestHandler<DeleteProdutoCommand, bool>
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IMediatorHandler _mediatorHandler;

        public DeleteProdutoCommandHandler(IProdutoRepository produtoRepository, IMediatorHandler mediatorHandler)
        {
            _produtoRepository = produtoRepository;
            _mediatorHandler = mediatorHandler;
        }

        public async Task<bool> Handle(DeleteProdutoCommand request, CancellationToken cancellationToken)
        {
            if (!ValidarComando(request)) return false;
            
            var result = await _produtoRepository.Delete(request.Id);

            if (result) return true;
            else
            {
                await _mediatorHandler.PublicarNotificacao(new DomainNotification("", "produto não encontrado!"));
                return false;
            }
        }

        private bool ValidarComando(Command message)
        {
            if (message.EhValido()) return true;


            foreach (var error in message.ValidationResult.Errors)
            {
                _mediatorHandler.PublicarNotificacao(new DomainNotification(message.MessageType, error.ToString()));
            }

            return false;
        }
    }
}
