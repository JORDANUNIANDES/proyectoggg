using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.API.Data;
using RestaurantManagement.API.DTOs;
using RestaurantManagement.API.Helpers;
using RestaurantManagement.API.Models;

namespace RestaurantManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public ClientesController(RestaurantDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes([FromQuery] string? buscar, [FromQuery] bool? soloActivos)
        {
            var query = _context.Clientes.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(c => c.Activo);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var term = buscar.Trim().ToLower();
                query = query.Where(c => c.Nombre.ToLower().Contains(term) ||
                                         c.Apellido.ToLower().Contains(term) ||
                                         c.Cedula.Contains(term) ||
                                         c.Email.ToLower().Contains(term));
            }

            var clientes = await query
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .Select(c => new ClienteDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Apellido = c.Apellido,
                    Cedula = c.Cedula,
                    Telefono = c.Telefono,
                    Email = c.Email,
                    Activo = c.Activo
                })
                .ToListAsync();

            return Ok(clientes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound(new { mensaje = "Cliente no encontrado." });

            return Ok(new ClienteDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Cedula = cliente.Cedula,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Activo = cliente.Activo
            });
        }

        [HttpPost]
        public async Task<ActionResult<ClienteDto>> CreateCliente([FromBody] ClienteCreateUpdateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Apellido))
                return BadRequest(new { mensaje = "El nombre y el apellido son obligatorios." });

            if (!EcuadorianValidator.ValidarCedula(dto.Cedula))
                return BadRequest(new { mensaje = "La cédula ingresada no es válida según el algoritmo de verificación ecuatoriano (10 dígitos)." });

            var existeCedula = await _context.Clientes.AnyAsync(c => c.Cedula == dto.Cedula.Trim());
            if (existeCedula)
                return BadRequest(new { mensaje = "Ya existe un cliente registrado con la misma cédula." });

            var cliente = new Cliente
            {
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                Cedula = dto.Cedula.Trim(),
                Telefono = dto.Telefono?.Trim() ?? string.Empty,
                Email = dto.Email?.Trim() ?? string.Empty,
                Activo = dto.Activo
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var resultDto = new ClienteDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Cedula = cliente.Cedula,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Activo = cliente.Activo
            };

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCliente(int id, [FromBody] ClienteCreateUpdateDto dto)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound(new { mensaje = "Cliente no encontrado." });

            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Apellido))
                return BadRequest(new { mensaje = "El nombre y el apellido son obligatorios." });

            if (!EcuadorianValidator.ValidarCedula(dto.Cedula))
                return BadRequest(new { mensaje = "La cédula ingresada no es válida según el algoritmo de verificación ecuatoriano." });

            var existeCedula = await _context.Clientes.AnyAsync(c => c.Cedula == dto.Cedula.Trim() && c.Id != id);
            if (existeCedula)
                return BadRequest(new { mensaje = "Ya existe otro cliente registrado con la misma cédula." });

            cliente.Nombre = dto.Nombre.Trim();
            cliente.Apellido = dto.Apellido.Trim();
            cliente.Cedula = dto.Cedula.Trim();
            cliente.Telefono = dto.Telefono?.Trim() ?? string.Empty;
            cliente.Email = dto.Email?.Trim() ?? string.Empty;
            cliente.Activo = dto.Activo;

            await _context.SaveChangesAsync();
            return Ok(new ClienteDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Cedula = cliente.Cedula,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Activo = cliente.Activo
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Pedidos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound(new { mensaje = "Cliente no encontrado." });

            if (cliente.Pedidos.Any())
            {
                return Conflict(new { mensaje = "No se puede eliminar el cliente porque tiene pedidos registrados en el historial. Puede desactivar el cliente en su lugar." });
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Cliente eliminado exitosamente." });
        }
    }
}
