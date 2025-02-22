using MediatR;
using WakeCommerce.Application.Commands;
using WakeCommerce.Core.Mediator;
using WakeCommerce.Core.Messages;
using WakeCommerce.Core.Messages.CommonMessages.Notifications;
using WakeCommerce.Domain.Entities;
using WakeCommerce.Domain.Repositories;

namespace WakeCommerce.Application.CommandHandlers
{
    public class CreateProdutoCommandHandler : IRequestHandler<CreateProdutoCommand, bool>
    {
        private readonly IMediatorHandler _mediatorHandler;
        private readonly IProdutoRepository _produtoRepository;
        public CreateProdutoCommandHandler(IMediatorHandler mediatorHandler, IProdutoRepository produtoRepository)
        {
            _mediatorHandler = mediatorHandler;
            _produtoRepository = produtoRepository;
        }

        public async Task<bool> Handle(CreateProdutoCommand message, CancellationToken cancellationToken)
        {
            if (!ValidarComando(message)) return false;

            var produto = new Produto()
            { 
                Estoque = message.Estoque,
                Descricao = message.Descricao,
                Nome = message.Nome,
                Preco = message.Preco
            };

            await _produtoRepository.Adicionar(produto);

            return true;
        }

        private bool ValidarComando(Command message)
        {
            if (message.EhValido()) return true;

            foreach (var error in message.ValidationResult.Errors)
            {
                _mediatorHandler.PublicarNotificacao(new DomainNotification(message.MessageType, error.ErrorMessage));
            }

            return false;
        }
    }
}
