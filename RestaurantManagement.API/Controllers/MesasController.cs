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
    public class MesasController : ControllerBase
    {
        private readonly IRestaurantService _service;

        public MesasController(IRestaurantService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene el listado de mesas.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MesaDto>))]
        public async Task<ActionResult<IEnumerable<MesaDto>>> GetMesas()
        {
            var mesas = await _service.GetMesasAsync();
            return Ok(mesas);
        }

        /// <summary>
        /// Obtiene una mesa por su ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MesaDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MesaDto>> GetMesa(int id)
        {
            var mesa = await _service.GetMesaByIdAsync(id);
            if (mesa == null)
            {
                return NotFound(new { message = $"No se encontró la mesa con ID {id}." });
            }
            return Ok(mesa);
        }

        /// <summary>
        /// Registra una nueva mesa.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MesaDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MesaDto>> CreateMesa([FromBody] MesaCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (result, error) = await _service.CreateMesaAsync(dto);
            if (error != null)
            {
                return BadRequest(new { message = error });
            }

            return CreatedAtAction(nameof(GetMesa), new { id = result!.Id }, result);
        }

        /// <summary>
        /// Actualiza una mesa existente.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMesa(int id, [FromBody] MesaUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, error) = await _service.UpdateMesaAsync(id, dto);
            if (!success)
            {
                if (error == "Mesa no encontrada.")
                {
                    return NotFound(new { message = error });
                }
                return BadRequest(new { message = error });
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina una mesa si no tiene pedidos registrados.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteMesa(int id)
        {
            var (success, error) = await _service.DeleteMesaAsync(id);
            if (!success)
            {
                if (error == "Mesa no encontrada.")
                {
                    return NotFound(new { message = error });
                }
                return Conflict(new { message = error });
            }

            return NoContent();
        }
    }
}
