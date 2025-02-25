using AutoMapper;
using WakeCommerce.Application.Commands;
using WakeCommerce.Application.Queries.Response;
using WakeCommerce.Domain.Entities;

namespace WakeCommerce.Application.Mappings
{
    public class ProdutoProfile : Profile
    {
        public ProdutoProfile()
        {
            // Mapeia de Command para Entity
            CreateMap<CreateProdutoCommand, Produto>();

            CreateMap<Produto, ProdutoResponse>();
        }
    }
}
