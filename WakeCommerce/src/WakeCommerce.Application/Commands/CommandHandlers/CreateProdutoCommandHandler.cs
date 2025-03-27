using System.Diagnostics;
using AutoMapper;
using MediatR;
using OpenTelemetry.Trace;
using WakeCommerce.Application.Commands;
using WakeCommerce.Application.Events;
using WakeCommerce.Application.Queries.Response;
using WakeCommerce.Core.Mediator;
using WakeCommerce.Core.Messages.CommonMessages.Notifications;
using WakeCommerce.Domain.Entities;
using WakeCommerce.Domain.Repositories;

namespace WakeCommerce.Application.CommandHandlers
{
    public class CreateProdutoCommandHandler : IRequestHandler<CreateProdutoCommand, ProdutoResponse?>
    {
        private readonly IMediatorHandler _mediatorHandler;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly Tracer _tracer;
        public CreateProdutoCommandHandler(IMediatorHandler mediatorHandler, IProdutoRepository produtoRepository, IMediator mediator, IMapper mapper, TracerProvider tracerProvider)
        {
            _mediatorHandler = mediatorHandler;
            _produtoRepository = produtoRepository;
            _mediator = mediator;
            _mapper = mapper;
            _tracer = tracerProvider.GetTracer("WakeCommerceActivitySource");
        }

        public async Task<ProdutoResponse?> Handle(CreateProdutoCommand message, CancellationToken cancellationToken)
        {
            using (var span = _tracer.StartActiveSpan(nameof(CreateProdutoCommandHandler)))
            {
                span.SetAttribute("command.type", nameof(CreateProdutoCommand));
                span.SetAttribute("service.name", "ProdutoService");

                if (!ValidarComando(message)) return null;

                var produto = _mapper.Map<Produto>(message);

                await _produtoRepository.Adicionar(produto);

                var response = _mapper.Map<ProdutoResponse>(produto);

                await _mediator.Publish(new ProdutoCreateEvent(response));

                span.SetAttribute("status", "success");

                return response;
            }
        }

        private bool ValidarComando(CreateProdutoCommand message)
        {
            if (message.EhValido()) return true;

            foreach (var error in message.ValidationResult.Errors)
            {
                _mediatorHandler.PublicarNotificacao(new DomainNotification("", error.ErrorMessage));
            }

            Activity.Current.AddEvent(new ActivityEvent(
                "ValidationFailed",
                tags: new ActivityTagsCollection(new List<KeyValuePair<string, object?>>
                {
                    new ("model.type", nameof(CreateProdutoCommand)),
                    new ("model.failed_field", nameof(message.Preco))
                })
            ));

            return false;
        }
    }
}
