using AutoMapper;
using MediatR;
using WakeCommerce.Application.Commands;
using WakeCommerce.Application.Queries.Response;
using WakeCommerce.Core.Mediator;
using WakeCommerce.Core.Messages.CommonMessages.Notifications;
using WakeCommerce.Domain.Repositories;

namespace WakeCommerce.Application.CommandHandlers
{
    public class UpdateProdutoCommandHandler : IRequestHandler<UpdateProdutoCommand, ProdutoResponse?>
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IMediatorHandler _mediatorHandler;
        private readonly IMapper _mapper;
        public UpdateProdutoCommandHandler(IProdutoRepository produtoRepository, IMediatorHandler mediatorHandler, IMapper mapper)
        {
            _produtoRepository = produtoRepository;
            _mediatorHandler = mediatorHandler;
            _mapper = mapper;
        }

        public async Task<ProdutoResponse?> Handle(UpdateProdutoCommand request, CancellationToken cancellationToken)
        {
            if (!ValidarComando(request)) return null;

            var produto = await _produtoRepository.GetById(request.Id);

            if (produto == null)
            {
                await _mediatorHandler.PublicarNotificacao(new DomainNotification("", "produto não encontrado!"));
                return null;
            }

            produto.Preco = request.Preco;
            produto.Nome   = request.Nome;
            produto.Descricao = request.Descricao;
            produto.Estoque = request.Estoque;

            await _produtoRepository.Update(produto);

            var response = _mapper.Map<ProdutoResponse>(produto);

            return response;
        }

        private bool ValidarComando(UpdateProdutoCommand message)
        {
            if (message.EhValido()) return true;


            foreach (var error in message.ValidationResult.Errors)
            {
                _mediatorHandler.PublicarNotificacao(new DomainNotification("", error.ToString()));
            }

            return false;
        }
    }
}
