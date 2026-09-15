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
    public class MesasController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public MesasController(RestaurantDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MesaDto>>> GetMesas([FromQuery] MesaEstado? estado, [FromQuery] bool? soloActivas)
        {
            var query = _context.Mesas.AsQueryable();

            if (soloActivas.HasValue && soloActivas.Value)
            {
                query = query.Where(m => m.Activo);
            }

            if (estado.HasValue)
            {
                query = query.Where(m => m.Estado == estado.Value);
            }

            var mesas = await query
                .OrderBy(m => m.NumeroMesa)
                .Select(m => new MesaDto
                {
                    Id = m.Id,
                    NumeroMesa = m.NumeroMesa,
                    Capacidad = m.Capacidad,
                    Estado = m.Estado,
                    Activo = m.Activo
                })
                .ToListAsync();

            return Ok(mesas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MesaDto>> GetMesa(int id)
        {
            var mesa = await _context.Mesas.FindAsync(id);
            if (mesa == null)
                return NotFound(new { mensaje = "Mesa no encontrada." });

            return Ok(new MesaDto
            {
                Id = mesa.Id,
                NumeroMesa = mesa.NumeroMesa,
                Capacidad = mesa.Capacidad,
                Estado = mesa.Estado,
                Activo = mesa.Activo
            });
        }

        [HttpPost]
        public async Task<ActionResult<MesaDto>> CreateMesa([FromBody] MesaCreateUpdateDto dto)
        {
            if (dto.NumeroMesa <= 0)
                return BadRequest(new { mensaje = "El número de mesa debe ser un entero positivo." });

            if (dto.Capacidad <= 0)
                return BadRequest(new { mensaje = "La capacidad de la mesa debe ser mayor a cero." });

            var existeNumero = await _context.Mesas.AnyAsync(m => m.NumeroMesa == dto.NumeroMesa);
            if (existeNumero)
                return BadRequest(new { mensaje = $"Ya existe la mesa número {dto.NumeroMesa}." });

            var mesa = new Mesa
            {
                NumeroMesa = dto.NumeroMesa,
                Capacidad = dto.Capacidad,
                Estado = dto.Estado,
                Activo = dto.Activo
            };

            _context.Mesas.Add(mesa);
            await _context.SaveChangesAsync();

            var resultDto = new MesaDto
            {
                Id = mesa.Id,
                NumeroMesa = mesa.NumeroMesa,
                Capacidad = mesa.Capacidad,
                Estado = mesa.Estado,
                Activo = mesa.Activo
            };

            return CreatedAtAction(nameof(GetMesa), new { id = mesa.Id }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMesa(int id, [FromBody] MesaCreateUpdateDto dto)
        {
            var mesa = await _context.Mesas.FindAsync(id);
            if (mesa == null)
                return NotFound(new { mensaje = "Mesa no encontrada." });

            if (dto.NumeroMesa <= 0)
                return BadRequest(new { mensaje = "El número de mesa debe ser un entero positivo." });

            if (dto.Capacidad <= 0)
                return BadRequest(new { mensaje = "La capacidad de la mesa debe ser mayor a cero." });

            var existeNumero = await _context.Mesas.AnyAsync(m => m.NumeroMesa == dto.NumeroMesa && m.Id != id);
            if (existeNumero)
                return BadRequest(new { mensaje = $"Ya existe otra mesa con el número {dto.NumeroMesa}." });

            mesa.NumeroMesa = dto.NumeroMesa;
            mesa.Capacidad = dto.Capacidad;
            mesa.Estado = dto.Estado;
            mesa.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(new MesaDto
            {
                Id = mesa.Id,
                NumeroMesa = mesa.NumeroMesa,
                Capacidad = mesa.Capacidad,
                Estado = mesa.Estado,
                Activo = mesa.Activo
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMesa(int id)
        {
            var mesa = await _context.Mesas
                .Include(m => m.Pedidos)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mesa == null)
                return NotFound(new { mensaje = "Mesa no encontrada." });

            if (mesa.Pedidos.Any())
            {
                return Conflict(new { mensaje = "No se puede eliminar la mesa porque tiene pedidos asociados en el historial. Puede inhabilitarla en su lugar." });
            }

            _context.Mesas.Remove(mesa);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Mesa eliminada exitosamente." });
        }
    }
}
