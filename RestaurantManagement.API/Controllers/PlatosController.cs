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
    public class PlatosController : ControllerBase
    {
        private readonly IRestaurantService _service;

        public PlatosController(IRestaurantService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene el listado de platos con filtros opcionales.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PlatoDto>))]
        public async Task<ActionResult<IEnumerable<PlatoDto>>> GetPlatos([FromQuery] bool? disponible, [FromQuery] string? categoria)
        {
            var platos = await _service.GetPlatosAsync(disponible, categoria);
            return Ok(platos);
        }

        /// <summary>
        /// Obtiene un plato por su ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlatoDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PlatoDto>> GetPlato(int id)
        {
            var plato = await _service.GetPlatoByIdAsync(id);
            if (plato == null)
            {
                return NotFound(new { message = $"No se encontró el plato con ID {id}." });
            }
            return Ok(plato);
        }

        /// <summary>
        /// Crea un nuevo plato.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PlatoDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PlatoDto>> CreatePlato([FromBody] PlatoCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (result, error) = await _service.CreatePlatoAsync(dto);
            if (error != null)
            {
                return BadRequest(new { message = error });
            }

            return CreatedAtAction(nameof(GetPlato), new { id = result!.Id }, result);
        }

        /// <summary>
        /// Actualiza un plato existente.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePlato(int id, [FromBody] PlatoUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, error) = await _service.UpdatePlatoAsync(id, dto);
            if (!success)
            {
                if (error == "Plato no encontrado.")
                {
                    return NotFound(new { message = error });
                }
                return BadRequest(new { message = error });
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina o desactiva un plato según si forma parte de historial de pedidos.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePlato(int id)
        {
            var (success, error) = await _service.DeletePlatoAsync(id);
            if (!success)
            {
                return NotFound(new { message = error });
            }

            if (error != null)
            {
                // Baja lógica realizada con mensaje descriptivo
                return Ok(new { message = error });
            }

            return NoContent();
        }
    }
}
