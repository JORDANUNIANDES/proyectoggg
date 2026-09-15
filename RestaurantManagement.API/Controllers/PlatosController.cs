using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.API.Data;
using RestaurantManagement.API.DTOs;
using RestaurantManagement.API.Models;

namespace RestaurantManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlatosController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public PlatosController(RestaurantDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlatoDto>>> GetPlatos([FromQuery] string? categoria, [FromQuery] bool? soloDisponibles, [FromQuery] bool? soloActivos)
        {
            var query = _context.Platos.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(p => p.Activo);
            }

            if (soloDisponibles.HasValue && soloDisponibles.Value)
            {
                query = query.Where(p => p.Disponible && p.Activo);
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                query = query.Where(p => p.Categoria.ToLower() == categoria.Trim().ToLower());
            }

            var platos = await query
                .OrderBy(p => p.Categoria)
                .ThenBy(p => p.Nombre)
                .Select(p => new PlatoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Categoria = p.Categoria,
                    Disponible = p.Disponible,
                    Activo = p.Activo
                })
                .ToListAsync();

            return Ok(platos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlatoDto>> GetPlato(int id)
        {
            var plato = await _context.Platos.FindAsync(id);
            if (plato == null)
                return NotFound(new { mensaje = "Plato no encontrado." });

            return Ok(new PlatoDto
            {
                Id = plato.Id,
                Nombre = plato.Nombre,
                Descripcion = plato.Descripcion,
                Precio = plato.Precio,
                Categoria = plato.Categoria,
                Disponible = plato.Disponible,
                Activo = plato.Activo
            });
        }

        [HttpPost]
        public async Task<ActionResult<PlatoDto>> CreatePlato([FromBody] PlatoCreateUpdateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new { mensaje = "El nombre del plato es obligatorio." });

            if (dto.Precio <= 0)
                return BadRequest(new { mensaje = "El precio del plato debe ser mayor a cero." });

            var plato = new Plato
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion?.Trim() ?? string.Empty,
                Precio = dto.Precio,
                Categoria = string.IsNullOrWhiteSpace(dto.Categoria) ? "General" : dto.Categoria.Trim(),
                Disponible = dto.Disponible,
                Activo = dto.Activo
            };

            _context.Platos.Add(plato);
            await _context.SaveChangesAsync();

            var resultDto = new PlatoDto
            {
                Id = plato.Id,
                Nombre = plato.Nombre,
                Descripcion = plato.Descripcion,
                Precio = plato.Precio,
                Categoria = plato.Categoria,
                Disponible = plato.Disponible,
                Activo = plato.Activo
            };

            return CreatedAtAction(nameof(GetPlato), new { id = plato.Id }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlato(int id, [FromBody] PlatoCreateUpdateDto dto)
        {
            var plato = await _context.Platos.FindAsync(id);
            if (plato == null)
                return NotFound(new { mensaje = "Plato no encontrado." });

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new { mensaje = "El nombre del plato es obligatorio." });

            if (dto.Precio <= 0)
                return BadRequest(new { mensaje = "El precio del plato debe ser mayor a cero." });

            plato.Nombre = dto.Nombre.Trim();
            plato.Descripcion = dto.Descripcion?.Trim() ?? string.Empty;
            plato.Precio = dto.Precio;
            plato.Categoria = string.IsNullOrWhiteSpace(dto.Categoria) ? "General" : dto.Categoria.Trim();
            plato.Disponible = dto.Disponible;
            plato.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(new PlatoDto
            {
                Id = plato.Id,
                Nombre = plato.Nombre,
                Descripcion = plato.Descripcion,
                Precio = plato.Precio,
                Categoria = plato.Categoria,
                Disponible = plato.Disponible,
                Activo = plato.Activo
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlato(int id)
        {
            var plato = await _context.Platos
                .Include(p => p.DetallesPedido)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plato == null)
                return NotFound(new { mensaje = "Plato no encontrado." });

            if (plato.DetallesPedido.Any())
            {
                return Conflict(new { mensaje = "No se puede eliminar el plato porque aparece en pedidos históricos. Puede desactivarlo o cambiar su disponibilidad en su lugar." });
            }

            _context.Platos.Remove(plato);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Plato eliminado exitosamente." });
        }
    }
}
