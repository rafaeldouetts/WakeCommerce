namespace WakeCommerce.Domain.Repository
{
    public interface IRedisRepository
    {
        Task SalvarProdutoNoRedisAsync(string chave, object valor);
        Task<string> ObterProdutoDoRedisAsync(string chave);
    }
}
