using MediatR;
using WakeCommerce.Application.Commands;
using WakeCommerce.Domain.Repositories;

namespace WakeCommerce.Application.CommandHandlers
{
    public class UpdateProdutoCommandHandler : IRequestHandler<UpdateProdutoCommand, bool>
    {
        private readonly IProdutoRepository _produtoRepository;

        public UpdateProdutoCommandHandler(IProdutoRepository produtoRepository)
        {
            _produtoRepository = produtoRepository;
        }

        public async Task<bool> Handle(UpdateProdutoCommand request, CancellationToken cancellationToken)
        {
            var produto = await _produtoRepository.GetById(request.Id);

            if (produto == null) return false;

            produto.Preco = request.Preco;
            produto.Nome   = request.Nome;
            produto.Descricao = request.Descricao;
            produto.Estoque = request.Estoque;

            await _produtoRepository.Update(produto);

            return true;
        }
    }
}
