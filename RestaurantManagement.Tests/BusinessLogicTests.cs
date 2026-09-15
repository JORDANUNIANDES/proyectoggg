using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RestaurantManagement.API.Data;
using RestaurantManagement.API.DTOs;
using RestaurantManagement.API.Services;
using Xunit;

namespace RestaurantManagement.Tests
{
    public class BusinessLogicTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var context = new ApplicationDbContext(options);
            return context;
        }

        [Theory]
        [InlineData("1710034065", true)]  // Cédula Ecuatoriana Válida
        [InlineData("1713394631", true)]  // Cédula Ecuatoriana Válida
        [InlineData("0910000009", true)]  // Cédula Ecuatoriana Válida
        [InlineData("1234567890", false)] // Inválida dígito verificador
        [InlineData("0923456", false)]    // Longitud incorrecta
        [InlineData("ABCDEFGHIJ", false)] // No numérica
        public void ValidarCedulaEcuatoriana(string cedula, bool expected)
        {
            bool actual = EcuadorianValidator.ValidarCedula(cedula);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task CreatePedido_CalculatesTotalCorrectlyFromDatabasePrices()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new RestaurantService(context);

            var cliente = new API.Models.Cliente { Nombre = "Juan", Apellido = "Pérez", Cedula = "1710034065", Estado = true };
            var plato1 = new API.Models.Plato { Nombre = "Lomo Saltado", Precio = 12.50m, Categoria = "Platos Fuertes", Disponible = true };
            var plato2 = new API.Models.Plato { Nombre = "Limonada", Precio = 4.50m, Categoria = "Bebidas", Disponible = true };

            context.Clientes.Add(cliente);
            context.Platos.AddRange(plato1, plato2);
            await context.SaveChangesAsync();

            var pedidoDto = new PedidoCreateDto
            {
                ClienteId = cliente.Id,
                Detalles = new System.Collections.Generic.List<PedidoDetalleCreateDto>
                {
                    new PedidoDetalleCreateDto { PlatoId = plato1.Id, Cantidad = 2 }, // 2 * 12.50 = 25.00
                    new PedidoDetalleCreateDto { PlatoId = plato2.Id, Cantidad = 1 }  // 1 * 4.50 = 4.50
                }
            };

            // Act
            var (result, error) = await service.CreatePedidoAsync(pedidoDto);

            // Assert
            Assert.Null(error);
            Assert.NotNull(result);
            Assert.Equal(29.50m, result!.Total);
            Assert.Equal(2, result.Detalles.Count);
        }

        [Fact]
        public async Task DeleteCliente_WithOrders_ReturnsErrorConflict()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new RestaurantService(context);

            var cliente = new API.Models.Cliente { Nombre = "Maria", Apellido = "Gómez", Cedula = "1713394631", Estado = true };
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            var pedido = new API.Models.Pedido { ClienteId = cliente.Id, Total = 10.00m, Estado = "Pendiente" };
            context.Pedidos.Add(pedido);
            await context.SaveChangesAsync();

            // Act
            var (success, error) = await service.DeleteClienteAsync(cliente.Id);

            // Assert
            Assert.False(success);
            Assert.Equal("No se puede eliminar el cliente porque tiene pedidos registrados.", error);
        }

        [Fact]
        public async Task DeletePlato_WithOrders_PerformsLogicalDelete()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new RestaurantService(context);

            var plato = new API.Models.Plato { Nombre = "Sopa", Precio = 5.00m, Categoria = "Sopas", Disponible = true };
            context.Platos.Add(plato);
            await context.SaveChangesAsync();

            var detalle = new API.Models.DetallePedido { PlatoId = plato.Id, Cantidad = 1, PrecioUnitario = 5.00m, Subtotal = 5.00m };
            context.DetallesPedido.Add(detalle);
            await context.SaveChangesAsync();

            // Act
            var (success, error) = await service.DeletePlatoAsync(plato.Id);

            // Assert
            Assert.True(success);
            Assert.NotNull(error);
            Assert.Contains("Ha sido marcado como no disponible", error);

            var platoDb = await context.Platos.FindAsync(plato.Id);
            Assert.NotNull(platoDb);
            Assert.False(platoDb!.Disponible);
        }
    }
}
