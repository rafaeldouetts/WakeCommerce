using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using WakeCommerce.Application.Queries.Response;


namespace WakeCommerce.Application.Commands
{
    public class UpdateProdutoCommand : IRequest<ProdutoResponse?>
    {
        public UpdateProdutoCommand()
        {
            Timestamp = DateTime.Now;
        }

        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal Preco { get; set; }
        public int Estoque { get; set; }
        [JsonIgnore]
        public ValidationResult? ValidationResult { get; set; }
        [JsonIgnore]
        public DateTime Timestamp { get; private set; }
        public bool EhValido()
        {
            ValidationResult = new UpdateProdutoValidation().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class UpdateProdutoValidation : AbstractValidator<UpdateProdutoCommand>
    {
        public UpdateProdutoValidation()
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
