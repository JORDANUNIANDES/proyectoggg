using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.API.DTOs;
using RestaurantManagement.API.Services;

namespace RestaurantManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ClientesController : ControllerBase
    {
        private readonly IRestaurantService _service;

        public ClientesController(IRestaurantService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene el listado de todos los clientes.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ClienteDto>))]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes()
        {
            var clientes = await _service.GetClientesAsync();
            return Ok(clientes);
        }

        /// <summary>
        /// Obtiene un cliente por su ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClienteDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id)
        {
            var cliente = await _service.GetClienteByIdAsync(id);
            if (cliente == null)
            {
                return NotFound(new { message = $"No se encontró el cliente con ID {id}." });
            }
            return Ok(cliente);
        }

        /// <summary>
        /// Registra un nuevo cliente.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ClienteDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ClienteDto>> CreateCliente([FromBody] ClienteCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (result, error) = await _service.CreateClienteAsync(dto);
            if (error != null)
            {
                return BadRequest(new { message = error });
            }

            return CreatedAtAction(nameof(GetCliente), new { id = result!.Id }, result);
        }

        /// <summary>
        /// Actualiza un cliente existente.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCliente(int id, [FromBody] ClienteUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, error) = await _service.UpdateClienteAsync(id, dto);
            if (!success)
            {
                if (error == "Cliente no encontrado.")
                {
                    return NotFound(new { message = error });
                }
                return BadRequest(new { message = error });
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina un cliente si no tiene pedidos históricos.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var (success, error) = await _service.DeleteClienteAsync(id);
            if (!success)
            {
                if (error == "Cliente no encontrado.")
                {
                    return NotFound(new { message = error });
                }
                // Regla de negocio: 409 Conflict si no se puede eliminar por restricciones históricas
                return Conflict(new { message = error });
            }

            return NoContent();
        }
    }
}
