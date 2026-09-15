using RestaurantManagement.API.Helpers;
using Xunit;

namespace RestaurantManagement.Tests
{
    public class EcuadorianValidatorTests
    {
        [Theory]
        [InlineData("1710034065", true)]
        [InlineData("0926629916", true)]
        [InlineData("1710034064", false)] // Invalid verifier digit
        [InlineData("1234567890", false)]
        [InlineData("171003406", false)]  // Only 9 digits
        [InlineData("17100340655", false)] // 11 digits
        [InlineData("ABCDEFGHIJ", false)]  // Non-numeric
        [InlineData("", false)]
        public void ValidarCedula_DeberiaRetornarResultadoEsperado(string cedula, bool resultadoEsperado)
        {
            var resultado = EcuadorianValidator.ValidarCedula(cedula);
            Assert.Equal(resultadoEsperado, resultado);
        }

        [Fact]
        public void ValidarCedula_CedulaNull_RetornaFalse()
        {
            var resultado = EcuadorianValidator.ValidarCedula(null!);
            Assert.False(resultado);
        }
    }
}
