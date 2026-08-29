using GestionClientes.Application.DTOs;
using GestionClientes.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GestionClientes.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClienteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> ObtenerTodos([FromQuery] string? busqueda, [FromQuery] bool? activo)
    {
        var clientes = await _clienteService.ObtenerTodosAsync(busqueda, activo);
        return Ok(clientes);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> ObtenerPorId(int id)
    {
        var cliente = await _clienteService.ObtenerPorIdAsync(id);
        if (cliente == null)
        {
            return NotFound(new { message = $"No se encontró el cliente con ID {id}" });
        }
        return Ok(cliente);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteDto>> Crear([FromBody] CrearClienteDto dto)
    {
        var cliente = await _clienteService.CrearAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = cliente.Id }, cliente);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteDto>> Actualizar(int id, [FromBody] ActualizarClienteDto dto)
    {
        var cliente = await _clienteService.ActualizarAsync(id, dto);
        return Ok(cliente);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int id)
    {
        var exito = await _clienteService.CambiarEstadoAsync(id, false);
        if (!exito)
        {
            return NotFound(new { message = $"No se encontró el cliente con ID {id}" });
        }
        return NoContent();
    }

    [HttpPatch("{id:int}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(int id)
    {
        var exito = await _clienteService.CambiarEstadoAsync(id, true);
        if (!exito)
        {
            return NotFound(new { message = $"No se encontró el cliente con ID {id}" });
        }
        return Ok(new { message = "Cliente activado correctamente" });
    }
}
