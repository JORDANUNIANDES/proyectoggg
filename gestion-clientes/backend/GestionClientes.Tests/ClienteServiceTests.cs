using GestionClientes.Application.DTOs;
using GestionClientes.Application.Services;
using GestionClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestionClientes.Tests;

public class ClienteServiceTests
{
    private ClientesDbContext CrearDbContextInMemory(string dbName)
    {
        var options = new DbContextOptionsBuilder<ClientesDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ClientesDbContext(options);
    }

    [Fact]
    public async Task CrearCliente_RucValido_RegistraExitosamente()
    {
        var context = CrearDbContextInMemory(nameof(CrearCliente_RucValido_RegistraExitosamente));
        var service = new ClienteService(context);

        var dto = new CrearClienteDto
        {
            Nombres = "Carlos",
            Apellidos = "Mendoza",
            Ruc = "1790016919001",
            Email = "carlos.mendoza@empresa.ec",
            Telefono = "0991234567",
            Direccion = "Av. Amazonas y Colón"
        };

        var resultado = await service.CrearAsync(dto);

        Assert.NotNull(resultado);
        Assert.True(resultado.Id > 0);
        Assert.Equal("Carlos", resultado.Nombres);
        Assert.True(resultado.Activo);
    }

    [Fact]
    public async Task CrearCliente_RucDuplicado_LanzaExcepcionConflict()
    {
        var context = CrearDbContextInMemory(nameof(CrearCliente_RucDuplicado_LanzaExcepcionConflict));
        var service = new ClienteService(context);

        var dto1 = new CrearClienteDto
        {
            Nombres = "Juan",
            Apellidos = "Pérez",
            Ruc = "1790016919001",
            Email = "juan.perez@test.com",
            Telefono = "0987654321",
            Direccion = "Quito Norte"
        };
        await service.CrearAsync(dto1);

        var dto2 = new CrearClienteDto
        {
            Nombres = "Ana",
            Apellidos = "Gómez",
            Ruc = "1790016919001", // Mismo RUC
            Email = "ana.gomez@test.com",
            Telefono = "0981112233",
            Direccion = "Quito Sur"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CrearAsync(dto2));
        Assert.Equal("Este RUC ya está registrado.", ex.Message);
    }

    [Fact]
    public async Task CrearCliente_CamposObligatoriosFaltantes_LanzaExcepcion()
    {
        var context = CrearDbContextInMemory(nameof(CrearCliente_CamposObligatoriosFaltantes_LanzaExcepcion));
        var service = new ClienteService(context);

        var dtoSinNombre = new CrearClienteDto
        {
            Nombres = "",
            Apellidos = "García",
            Ruc = "1790016919001",
            Email = "correo@valido.com",
            Direccion = "Guayaquil Centro"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CrearAsync(dtoSinNombre));
    }

    [Fact]
    public async Task CrearCliente_EmailInvalido_LanzaExcepcion()
    {
        var context = CrearDbContextInMemory(nameof(CrearCliente_EmailInvalido_LanzaExcepcion));
        var service = new ClienteService(context);

        var dtoEmailInvalido = new CrearClienteDto
        {
            Nombres = "Luis",
            Apellidos = "Salazar",
            Ruc = "1790016919001",
            Email = "email-invalido-sin-arroba",
            Direccion = "Cuenca Este"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CrearAsync(dtoEmailInvalido));
    }

    [Fact]
    public async Task CambiarEstado_DesactivarYActivarCliente_ActualizaCorrectamente()
    {
        var context = CrearDbContextInMemory(nameof(CambiarEstado_DesactivarYActivarCliente_ActualizaCorrectamente));
        var service = new ClienteService(context);

        var cliente = await service.CrearAsync(new CrearClienteDto
        {
            Nombres = "Maria",
            Apellidos = "Torres",
            Ruc = "1760001550001",
            Email = "maria.torres@empresa.com",
            Direccion = "Ambato Centro"
        });

        // Desactivar
        var exitoDesactivar = await service.CambiarEstadoAsync(cliente.Id, false);
        Assert.True(exitoDesactivar);

        var clienteInactivo = await service.ObtenerPorIdAsync(cliente.Id);
        Assert.NotNull(clienteInactivo);
        Assert.False(clienteInactivo.Activo);

        // Activar
        var exitoActivar = await service.CambiarEstadoAsync(cliente.Id, true);
        Assert.True(exitoActivar);

        var clienteActivo = await service.ObtenerPorIdAsync(cliente.Id);
        Assert.NotNull(clienteActivo);
        Assert.True(clienteActivo.Activo);
    }

    [Fact]
    public async Task ObtenerPorId_ClienteInexistente_RetornaNull()
    {
        var context = CrearDbContextInMemory(nameof(ObtenerPorId_ClienteInexistente_RetornaNull));
        var service = new ClienteService(context);

        var resultado = await service.ObtenerPorIdAsync(999);
        Assert.Null(resultado);
    }
}
