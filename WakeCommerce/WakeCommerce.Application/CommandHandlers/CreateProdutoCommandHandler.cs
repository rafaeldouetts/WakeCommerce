﻿using AutoMapper;
using MediatR;
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
        public CreateProdutoCommandHandler(IMediatorHandler mediatorHandler, IProdutoRepository produtoRepository, IMediator mediator, IMapper mapper)
        {
            _mediatorHandler = mediatorHandler;
            _produtoRepository = produtoRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ProdutoResponse?> Handle(CreateProdutoCommand message, CancellationToken cancellationToken)
        {
            if (!ValidarComando(message)) return null;

            var produto = _mapper.Map<Produto>(message);

            await _produtoRepository.Adicionar(produto);

            var response = _mapper.Map<ProdutoResponse>(produto);

            await _mediator.Publish(new ProdutoCreateEvent(response));

            return response;
        }

        private bool ValidarComando(CreateProdutoCommand message)
        {
            if (message.EhValido()) return true;

            foreach (var error in message.ValidationResult.Errors)
            {
                _mediatorHandler.PublicarNotificacao(new DomainNotification("", error.ErrorMessage));
            }

            return false;
        }
    }
}
