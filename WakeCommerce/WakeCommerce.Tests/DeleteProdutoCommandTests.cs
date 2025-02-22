
using WakeCommerce.Application.Commands;

namespace WakeCommerce.Unit.Tests
{
    public class DeleteProdutoCommandTests
    {
        [Fact]
        public void EhValido_DeveRetornarVerdadeiro_QuandoIdForMaiorQueZero()
        {
            // Arrange
            var command = new DeleteProdutoCommand(1);

            // Act
            var resultado = command.EhValido();

            // Assert
            Assert.Empty(command.ValidationResult.Errors);
            Assert.True(resultado);
        }

        [Fact]
        public void EhValido_DeveRetornarFalso_QuandoIdForZero()
        {
            // Arrange
            var command = new DeleteProdutoCommand(0);

            // Act
            var resultado = command.EhValido();

            // Assert
            Assert.False(resultado);
            Assert.Equal(command.ValidationResult.Errors.First().ErrorMessage, "O Id deve ser maior que 0");
        }

        [Fact]
        public void EhValido_DeveRetornarFalso_QuandoIdForNegativo()
        {
            // Arrange
            var command = new DeleteProdutoCommand(-5);

            // Act
            var resultado = command.EhValido();

            // Assert
            Assert.False(resultado);
            Assert.Equal(command.ValidationResult.Errors.First().ErrorMessage, "O Id deve ser maior que 0");
        }
    }
}
