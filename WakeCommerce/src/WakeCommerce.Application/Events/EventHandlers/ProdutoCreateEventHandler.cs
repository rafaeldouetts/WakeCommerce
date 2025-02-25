using MediatR;
using WakeCommerce.Domain.Repository;

namespace WakeCommerce.Application.Events.EventHandlers
{
    public class ProdutoCreateEventHandler : INotificationHandler<ProdutoCreateEvent>
    {
        private readonly IRedisRepository _redisRepository;

        public ProdutoCreateEventHandler(IRedisRepository redisRepository)
        {
            _redisRepository = redisRepository;
        }

        public Task Handle(ProdutoCreateEvent notification, CancellationToken cancellationToken)
        {
            _redisRepository.SalvarProdutoNoRedisAsync(notification.Produto.Id.ToString(), notification.Produto);
            _redisRepository.SalvarProdutoNoRedisAsync(notification.Produto.Nome.ToString(), notification.Produto);

            return Task.CompletedTask;
        }
    }
}
