using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WakeCommerce.Domain.Entities;


namespace WakeCommerce.Infrastructure.Data
{
    public class WakeCommerceDbContext : DbContext
    {
        public DbSet<Produto> Produtos { get; set; }
        
        public WakeCommerceDbContext(DbContextOptions<WakeCommerceDbContext> options)
                : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ProdutoMap
            builder.Entity<Produto>()
                .HasKey(t => t.Id);
        }

        public void UploadProdutos()
        {
            if (!Produtos.Any())  // Verifica se já existem produtos na base
            {
                Produtos.AddRange(
                    new Produto { Nome = "Produto 1", Preco = 10.00m, Descricao = "Descrição 1", Estoque = 10},
                    new Produto { Nome = "Produto 2", Preco = 20.00m, Descricao = "Descrição 1", Estoque = 10 },
                    new Produto { Nome = "Produto 3", Preco = 30.00m, Descricao = "Descrição 1", Estoque = 10 },
                    new Produto { Nome = "Produto 4", Preco = 40.00m, Descricao = "Descrição 1" , Estoque = 10 },
                    new Produto { Nome = "Produto 5", Preco = 50.00m, Descricao = "Descrição 1" , Estoque = 10 }
                );

                SaveChanges();  // Salva os dados na base
            }
        }
    }
}
