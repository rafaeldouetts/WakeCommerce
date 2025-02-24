using System.Text.Json;
using StackExchange.Redis;
using WakeCommerce.Domain.Repository;

namespace WakeCommerce.Infrastructure.Repository
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        public RedisRepository(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = _redis.GetDatabase();
        }

        public async Task<string> ObterProdutoDoRedisAsync(string chave)
        {
            var produtoJson = await _database.StringGetAsync(chave);

            if (produtoJson.IsNullOrEmpty)
            {
                return null;
            }

            return produtoJson;
        }

        public async Task SalvarProdutoNoRedisAsync(string chave, object valor)
        {
            var value = JsonSerializer.Serialize(valor);

            await _database.StringSetAsync(chave, value);
        }
    }
}
