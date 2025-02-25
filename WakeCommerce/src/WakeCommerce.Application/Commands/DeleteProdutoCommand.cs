

using FluentValidation;
using WakeCommerce.Core.Messages;

namespace WakeCommerce.Application.Commands
{
    public class DeleteProdutoCommand : Command
    {
        public int Id { get; set; }

        public DeleteProdutoCommand(int id)
        {
            Id = id;
        }

        public override bool EhValido()
        {
            ValidationResult = new DeleteProdutoValidation().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class DeleteProdutoValidation : AbstractValidator<DeleteProdutoCommand>
    {
        public DeleteProdutoValidation()
        {
            RuleFor(c => c.Id)
                .GreaterThan(0)
                .WithMessage("O Id deve ser maior que 0");
        }
    }
}
