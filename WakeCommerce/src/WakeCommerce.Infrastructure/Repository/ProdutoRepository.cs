
using System.Diagnostics;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using WakeCommerce.Domain.Entities;
using WakeCommerce.Domain.Repositories;
using WakeCommerce.Infrastructure.Data;

namespace WakeCommerce.Infrastructure.Repository
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly WakeCommerceDbContext _context;
        private readonly Tracer _tracer;
        public ProdutoRepository(WakeCommerceDbContext context, TracerProvider tracerProvider)
        {
            _context = context;
            _tracer = tracerProvider.GetTracer("WakeCommerceActivitySource");
        }

        public async Task Adicionar(Produto produto)
        {
            using (var span = _tracer.StartActiveSpan(nameof(IProdutoRepository)))
            {
                span.SetAttribute("command.type", nameof(ProdutoRepository));
                span.SetAttribute("service.name", "ProdutoService");
    
                await _context.Produtos.AddAsync(produto);
                await _context.SaveChangesAsync();

                await _context.DisposeAsync();

                span.AddEvent("Salvo no SQL Server");
            }
        }

        public async Task<bool> Delete(int id)
        {
            var produto = await GetById(id);

            if (produto == null) return false;

            _context.Produtos.Remove(produto);

            _context.SaveChanges();

            return true;
        }

        public Task<List<Produto>> GetAll(bool descending, string orderBy)
        {
            IQueryable<Produto> query = _context.Set<Produto>();
            query = orderBy?.ToLower() switch
            {
                "nome" => descending
                    ? query.OrderByDescending(p => p.Nome)
                    : query.OrderBy(p => p.Nome),
                "preco" => descending
                    ? query.OrderByDescending(p => p.Preco)
                    : query.OrderBy(p => p.Preco),
                "estoque" => descending
                    ? query.OrderByDescending(p => p.Estoque)
                    : query.OrderBy(p => p.Estoque),
                "descricao" => descending
                    ? query.OrderByDescending(p => p.Descricao)
                    : query.OrderBy(p => p.Descricao),

                _ => descending
                    ? query.OrderByDescending(p => p.Id)
                    : query.OrderBy(p => p.Id)
            };

            return query.ToListAsync();
        }

        public Task<Produto?> GetById(int id)
        {
            return _context.Produtos
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public Task<Produto?> GetByNome(string nome)
        {
            return _context.Produtos
                .AsNoTracking()
                .Where(x => x.Nome == nome)
                .FirstOrDefaultAsync();
        }

        public async Task Update(Produto produto)
        {
            _context.Produtos.Update(produto);

            _context.SaveChanges();
        }
    }
}
