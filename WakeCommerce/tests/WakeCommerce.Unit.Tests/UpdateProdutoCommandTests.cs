using WakeCommerce.Application.Commands;

namespace WakeCommerce.Unit.Tests
{
    public class UpdateProdutoCommandTests
    {
        [Fact]

        public void EhValido_DeveRetornarVerdadeiro_QuandoDadosForemValidos()
        {
            // Arrange
            var command = new UpdateProdutoCommand
            {
                Nome = "Produto Teste",
                Preco = 100m,
                Estoque = 10
            };

            // Act
            var resultado = command.EhValido();

            // Assert
            Assert.Empty(command.ValidationResult.Errors);
            Assert.True(resultado);
        }

        [Fact]
        public void EhValido_DeveRetornarFalso_QuandoNomeEstiverVazio()
        {
            // Arrange
            var command = new UpdateProdutoCommand
            {
                Nome = "",
                Preco = 100m,
                Estoque = 10
            };

            // Act
            var resultado = command.EhValido();

            // Assert
            Assert.False(resultado);
            Assert.Equal(command.ValidationResult.Errors.First().ErrorMessage, "O nome não pode ser vazio");
        }

        [Fact]
        public void EhValido_DeveRetornarFalso_QuandoPrecoForMenorQueZero()
        {
            // Arrange
            var command = new UpdateProdutoCommand
            {
                Nome = "Produto Teste",
                Preco = -1m,
                Estoque = 10
            };

            // Act
            var resultado = command.EhValido();

            // Assert
            Assert.False(resultado);
            Assert.Equal(command.ValidationResult.Errors.First().ErrorMessage, "O preço não pode ser menor que 0");

        }
    }
}
