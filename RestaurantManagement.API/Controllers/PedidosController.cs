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
    public class PedidosController : ControllerBase
    {
        private readonly IRestaurantService _service;

        public PedidosController(IRestaurantService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene el listado de pedidos o filtrado por cliente.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PedidoDto>))]
        public async Task<ActionResult<IEnumerable<PedidoDto>>> GetPedidos([FromQuery] int? clienteId)
        {
            var pedidos = await _service.GetPedidosAsync(clienteId);
            return Ok(pedidos);
        }

        /// <summary>
        /// Obtiene un pedido por su ID con el detalle de items.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PedidoDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PedidoDto>> GetPedido(int id)
        {
            var pedido = await _service.GetPedidoByIdAsync(id);
            if (pedido == null)
            {
                return NotFound(new { message = $"No se encontró el pedido con ID {id}." });
            }
            return Ok(pedido);
        }

        /// <summary>
        /// Registra un nuevo pedido con validación de precios en el servidor y cálculo automático.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PedidoDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PedidoDto>> CreatePedido([FromBody] PedidoCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (result, error) = await _service.CreatePedidoAsync(dto);
            if (error != null)
            {
                return BadRequest(new { message = error });
            }

            return CreatedAtAction(nameof(GetPedido), new { id = result!.Id }, result);
        }

        /// <summary>
        /// Cambia el estado de un pedido (Pendiente, En preparación, Servido, Pagado, Cancelado).
        /// </summary>
        [HttpPut("{id}/estado")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEstadoPedido(int id, [FromBody] PedidoUpdateEstadoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, error) = await _service.UpdateEstadoPedidoAsync(id, dto.Estado);
            if (!success)
            {
                if (error == "Pedido no encontrado.")
                {
                    return NotFound(new { message = error });
                }
                return BadRequest(new { message = error });
            }

            return NoContent();
        }

        /// <summary>
        /// Cancela un pedido preservando la información histórica.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelarPedido(int id)
        {
            var (success, error) = await _service.CancelarPedidoAsync(id);
            if (!success)
            {
                return NotFound(new { message = error });
            }

            return NoContent();
        }

        /// <summary>
        /// Obtiene los pedidos asociados a un cliente específico.
        /// </summary>
        [HttpGet("cliente/{clienteId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PedidoDto>))]
        public async Task<ActionResult<IEnumerable<PedidoDto>>> GetPedidosPorCliente(int clienteId)
        {
            var pedidos = await _service.GetPedidosAsync(clienteId);
            return Ok(pedidos);
        }
    }
}
