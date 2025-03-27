using MediatR;
using OpenTelemetry.Trace;
using WakeCommerce.Application.Commands;
using WakeCommerce.Domain.Repository;

namespace WakeCommerce.Application.Events.EventHandlers
{
    public class ProdutoCreateEventHandler : INotificationHandler<ProdutoCreateEvent>
    {
        private readonly IRedisRepository _redisRepository;
        private readonly Tracer _tracer;
        public ProdutoCreateEventHandler(IRedisRepository redisRepository, TracerProvider tracerProvider)
        {
            _redisRepository = redisRepository;
            _tracer = tracerProvider.GetTracer("WakeCommerceActivitySource");
        }

        public async Task Handle(ProdutoCreateEvent notification, CancellationToken cancellationToken)
        {
            using (var span = _tracer.StartActiveSpan(nameof(ProdutoCreateEventHandler)))
            {
                span.SetAttribute("command.type", nameof(ProdutoCreateEvent));
                span.SetAttribute("service.name", "ProdutoService");
                await _redisRepository.SalvarProdutoNoRedisAsync(notification.Produto.Id.ToString(), notification.Produto);
                await _redisRepository.SalvarProdutoNoRedisAsync(notification.Produto.Nome.ToString(), notification.Produto);

                span.AddEvent("Salvo no Redis");
            }
        }
    }
}
