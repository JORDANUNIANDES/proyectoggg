using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RestaurantManagement.API.Controllers;
using RestaurantManagement.API.Data;
using RestaurantManagement.API.DTOs;
using RestaurantManagement.API.Models;
using Xunit;

namespace RestaurantManagement.Tests
{
    public class BusinessRulesTests
    {
        private RestaurantDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<RestaurantDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var context = new RestaurantDbContext(options);
            return context;
        }

        [Fact]
        public async Task CreateCliente_CedulaInvalida_RetornaBadRequest()
        {
            var context = GetInMemoryDbContext(nameof(CreateCliente_CedulaInvalida_RetornaBadRequest));
            var controller = new ClientesController(context);

            var dto = new ClienteCreateUpdateDto
            {
                Nombre = "Test",
                Apellido = "User",
                Cedula = "1234567890", // Invalid Cédula
                Email = "test@example.com"
            };

            var result = await controller.CreateCliente(dto);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteCliente_ConPedidosHistoricos_RetornaConflict409()
        {
            var context = GetInMemoryDbContext(nameof(DeleteCliente_ConPedidosHistoricos_RetornaConflict409));
            var cliente = new Cliente { Id = 1, Nombre = "Juan", Apellido = "Perez", Cedula = "1710034065" };
            context.Clientes.Add(cliente);
            context.Pedidos.Add(new Pedido { Id = 1, ClienteId = 1, Fecha = DateTime.Now, Total = 10m });
            await context.SaveChangesAsync();

            var controller = new ClientesController(context);
            var result = await controller.DeleteCliente(1);

            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.NotNull(conflictResult.Value);
        }

        [Fact]
        public async Task CreatePlato_PrecioCeroOMenor_RetornaBadRequest()
        {
            var context = GetInMemoryDbContext(nameof(CreatePlato_PrecioCeroOMenor_RetornaBadRequest));
            var controller = new PlatosController(context);

            var dto = new PlatoCreateUpdateDto
            {
                Nombre = "Plato Gratuito",
                Precio = 0m,
                Categoria = "Prueba"
            };

            var result = await controller.CreatePlato(dto);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeletePlato_EnDetallesPedido_RetornaConflict409()
        {
            var context = GetInMemoryDbContext(nameof(DeletePlato_EnDetallesPedido_RetornaConflict409));
            var plato = new Plato { Id = 1, Nombre = "Sopa", Precio = 3.50m, Categoria = "Entradas" };
            context.Platos.Add(plato);
            context.DetallesPedido.Add(new DetallePedido { Id = 1, PedidoId = 10, PlatoId = 1, Cantidad = 2, PrecioUnitario = 3.50m, Subtotal = 7.00m });
            await context.SaveChangesAsync();

            var controller = new PlatosController(context);
            var result = await controller.DeletePlato(1);

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task CreatePedido_CalculaTotalEnBackendYSincronizaMesa()
        {
            var context = GetInMemoryDbContext(nameof(CreatePedido_CalculaTotalEnBackendYSincronizaMesa));

            var cliente = new Cliente { Id = 1, Nombre = "Maria", Apellido = "Lopez", Cedula = "0926629916", Activo = true };
            var mesa = new Mesa { Id = 1, NumeroMesa = 1, Capacidad = 4, Estado = MesaEstado.Disponible, Activo = true };
            var plato1 = new Plato { Id = 1, Nombre = "Plato 1", Precio = 10.00m, Disponible = true, Activo = true };
            var plato2 = new Plato { Id = 2, Nombre = "Plato 2", Precio = 5.00m, Disponible = true, Activo = true };

            context.Clientes.Add(cliente);
            context.Mesas.Add(mesa);
            context.Platos.AddRange(plato1, plato2);
            await context.SaveChangesAsync();

            var controller = new PedidosController(context);
            var dto = new PedidoCreateDto
            {
                ClienteId = 1,
                MesaId = 1,
                Detalles = new List<DetallePedidoCreateDto>
                {
                    new DetallePedidoCreateDto { PlatoId = 1, Cantidad = 2 }, // 2 * 10 = 20
                    new DetallePedidoCreateDto { PlatoId = 2, Cantidad = 3 }  // 3 * 5 = 15
                }
            };

            var result = await controller.CreatePedido(dto);
            var createdAtAction = Assert.IsType<CreatedAtActionResult>(result.Result);
            var pedidoDto = Assert.IsType<PedidoDto>(createdAtAction.Value);

            Assert.Equal(35.00m, pedidoDto.Total); // Total strictly calculated on backend

            // Check table state automatically changed to Ocupada
            var mesaDb = await context.Mesas.FindAsync(1);
            Assert.Equal(MesaEstado.Ocupada, mesaDb!.Estado);
        }

        [Fact]
        public async Task UpdateEstadoPedido_CompletadoACancelado_Forbidden()
        {
            var context = GetInMemoryDbContext(nameof(UpdateEstadoPedido_CompletadoACancelado_Forbidden));
            var pedido = new Pedido { Id = 1, ClienteId = 1, Estado = PedidoEstado.Completado, Total = 20m };
            context.Pedidos.Add(pedido);
            await context.SaveChangesAsync();

            var controller = new PedidosController(context);
            var updateDto = new PedidoEstadoUpdateDto { NuevoEstado = PedidoEstado.Cancelado };

            var result = await controller.UpdateEstadoPedido(1, updateDto);
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
