using WakeCommerce.Domain.Entities;

namespace WakeCommerce.Domain.Repositories
{
    public interface IProdutoRepository
    {
        Task Adicionar(Produto produto);
        Task<bool> Delete(int id);
        Task<List<Produto>> GetAll(bool descending, string orderBy);
        Task<Produto?> GetById(int id);
        Task<Produto?> GetByNome(string nome);
        Task Update(Produto produto);
    }
}
