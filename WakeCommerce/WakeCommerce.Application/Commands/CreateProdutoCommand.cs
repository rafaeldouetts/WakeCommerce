﻿
using FluentValidation;
using WakeCommerce.Core.Messages;

namespace WakeCommerce.Application.Commands
{
    public class CreateProdutoCommand : Command
    {
        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public int Estoque { get; set; }

        public override bool EhValido()
        {
            ValidationResult = new CreateProdutoValidation().Validate(this);
            return ValidationResult.IsValid;
        }

    }

    public class CreateProdutoValidation : AbstractValidator<CreateProdutoCommand>
    {
        public CreateProdutoValidation()
        {
            RuleFor(c => c.Nome)
                .NotEmpty()
                .WithMessage("O nome não pode ser vazio");

            RuleFor(c => c.Preco)
            .NotEmpty()
            .WithMessage("O preço não pode ser vazio");
            
            RuleFor(c => c.Preco)
            .GreaterThan(0)
            .WithMessage("O preço não pode ser menor que 0");
        }
    }
}
