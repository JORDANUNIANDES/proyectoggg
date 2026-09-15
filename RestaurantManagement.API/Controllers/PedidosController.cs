using System;
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
    public class PedidosController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public PedidosController(RestaurantDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PedidoDto>>> GetPedidos([FromQuery] PedidoEstado? estado, [FromQuery] int? clienteId, [FromQuery] DateTime? fecha)
        {
            var query = _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Mesa)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .AsQueryable();

            if (estado.HasValue)
            {
                query = query.Where(p => p.Estado == estado.Value);
            }

            if (clienteId.HasValue)
            {
                query = query.Where(p => p.ClienteId == clienteId.Value);
            }

            if (fecha.HasValue)
            {
                var dateOnly = fecha.Value.Date;
                query = query.Where(p => p.Fecha.Date == dateOnly);
            }

            var pedidos = await query
                .OrderByDescending(p => p.Fecha)
                .Select(p => new PedidoDto
                {
                    Id = p.Id,
                    ClienteId = p.ClienteId,
                    ClienteNombreCompleto = $"{p.Cliente.Nombre} {p.Cliente.Apellido}",
                    ClienteCedula = p.Cliente.Cedula,
                    MesaId = p.MesaId,
                    NumeroMesa = p.Mesa != null ? p.Mesa.NumeroMesa : (int?)null,
                    Fecha = p.Fecha,
                    Estado = p.Estado,
                    Total = p.Total,
                    Detalles = p.Detalles.Select(d => new DetallePedidoDto
                    {
                        Id = d.Id,
                        PlatoId = d.PlatoId,
                        PlatoNombre = d.Plato.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal
                    }).ToList()
                })
                .ToListAsync();

            return Ok(pedidos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PedidoDto>> GetPedido(int id)
        {
            var p = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Mesa)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (p == null)
                return NotFound(new { mensaje = "Pedido no encontrado." });

            return Ok(new PedidoDto
            {
                Id = p.Id,
                ClienteId = p.ClienteId,
                ClienteNombreCompleto = $"{p.Cliente.Nombre} {p.Cliente.Apellido}",
                ClienteCedula = p.Cliente.Cedula,
                MesaId = p.MesaId,
                NumeroMesa = p.Mesa != null ? p.Mesa.NumeroMesa : (int?)null,
                Fecha = p.Fecha,
                Estado = p.Estado,
                Total = p.Total,
                Detalles = p.Detalles.Select(d => new DetallePedidoDto
                {
                    Id = d.Id,
                    PlatoId = d.PlatoId,
                    PlatoNombre = d.Plato.Nombre,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<ActionResult<PedidoDto>> CreatePedido([FromBody] PedidoCreateDto dto)
        {
            if (dto.Detalles == null || !dto.Detalles.Any())
                return BadRequest(new { mensaje = "No se puede registrar un pedido sin detalles/platos." });

            if (dto.Detalles.Any(d => d.Cantidad <= 0))
                return BadRequest(new { mensaje = "Todas las cantidades en los detalles del pedido deben ser mayores a cero." });

            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null || !cliente.Activo)
                return BadRequest(new { mensaje = "El cliente seleccionado no existe o no está activo." });

            Mesa? mesa = null;
            if (dto.MesaId.HasValue)
            {
                mesa = await _context.Mesas.FindAsync(dto.MesaId.Value);
                if (mesa == null || !mesa.Activo)
                    return BadRequest(new { mensaje = "La mesa seleccionada no existe o no está activa." });

                if (mesa.Estado == MesaEstado.Ocupada)
                    return BadRequest(new { mensaje = $"La mesa #{mesa.NumeroMesa} se encuentra actualmente ocupada." });
            }

            var platoIds = dto.Detalles.Select(d => d.PlatoId).Distinct().ToList();
            var platosDict = await _context.Platos
                .Where(p => platoIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var item in dto.Detalles)
            {
                if (!platosDict.TryGetValue(item.PlatoId, out var plato) || !plato.Activo)
                    return BadRequest(new { mensaje = $"El plato con ID {item.PlatoId} no existe o no está activo." });

                if (!plato.Disponible)
                    return BadRequest(new { mensaje = $"El plato '{plato.Nombre}' no se encuentra disponible actualmente." });
            }

            // Using transaction for data integrity
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pedido = new Pedido
                {
                    ClienteId = dto.ClienteId,
                    MesaId = dto.MesaId,
                    Fecha = DateTime.Now,
                    Estado = PedidoEstado.Pendiente,
                    Total = 0m
                };

                decimal totalCalculado = 0m;

                foreach (var detailDto in dto.Detalles)
                {
                    var plato = platosDict[detailDto.PlatoId];
                    var subtotal = plato.Precio * detailDto.Cantidad;
                    totalCalculado += subtotal;

                    pedido.Detalles.Add(new DetallePedido
                    {
                        PlatoId = detailDto.PlatoId,
                        Cantidad = detailDto.Cantidad,
                        PrecioUnitario = plato.Precio,
                        Subtotal = subtotal
                    });
                }

                pedido.Total = totalCalculado;
                _context.Pedidos.Add(pedido);

                if (mesa != null)
                {
                    mesa.Estado = MesaEstado.Ocupada;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Load navigation for output DTO
                await _context.Entry(pedido).Reference(p => p.Cliente).LoadAsync();
                if (pedido.MesaId.HasValue)
                    await _context.Entry(pedido).Reference(p => p.Mesa).LoadAsync();

                foreach (var det in pedido.Detalles)
                    await _context.Entry(det).Reference(d => d.Plato).LoadAsync();

                var resultDto = new PedidoDto
                {
                    Id = pedido.Id,
                    ClienteId = pedido.ClienteId,
                    ClienteNombreCompleto = $"{pedido.Cliente.Nombre} {pedido.Cliente.Apellido}",
                    ClienteCedula = pedido.Cliente.Cedula,
                    MesaId = pedido.MesaId,
                    NumeroMesa = pedido.Mesa?.NumeroMesa,
                    Fecha = pedido.Fecha,
                    Estado = pedido.Estado,
                    Total = pedido.Total,
                    Detalles = pedido.Detalles.Select(d => new DetallePedidoDto
                    {
                        Id = d.Id,
                        PlatoId = d.PlatoId,
                        PlatoNombre = d.Plato.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal
                    }).ToList()
                };

                return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, resultDto);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> UpdateEstadoPedido(int id, [FromBody] PedidoEstadoUpdateDto dto)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Mesa)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
                return NotFound(new { mensaje = "Pedido no encontrado." });

            if (pedido.Estado == PedidoEstado.Completado || pedido.Estado == PedidoEstado.Cancelado)
            {
                return BadRequest(new { mensaje = $"El pedido #{pedido.Id} ya se encuentra en estado '{pedido.Estado}' y no puede ser modificado." });
            }

            // Valid transitions
            if (pedido.Estado == PedidoEstado.Pendiente)
            {
                if (dto.NuevoEstado != PedidoEstado.EnPreparacion &&
                    dto.NuevoEstado != PedidoEstado.Completado &&
                    dto.NuevoEstado != PedidoEstado.Cancelado)
                {
                    return BadRequest(new { mensaje = "Transición de estado no válida para un pedido Pendiente." });
                }
            }
            else if (pedido.Estado == PedidoEstado.EnPreparacion)
            {
                if (dto.NuevoEstado != PedidoEstado.Completado &&
                    dto.NuevoEstado != PedidoEstado.Cancelado)
                {
                    return BadRequest(new { mensaje = "Transición de estado no válida para un pedido En Preparación." });
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                pedido.Estado = dto.NuevoEstado;

                // Sync table state if order reaches terminal state
                if ((dto.NuevoEstado == PedidoEstado.Completado || dto.NuevoEstado == PedidoEstado.Cancelado) && pedido.MesaId.HasValue)
                {
                    var mesaId = pedido.MesaId.Value;
                    // Check if there are other pending or in-preparation orders on this table
                    var tieneOtrosPedidosActivos = await _context.Pedidos.AnyAsync(p =>
                        p.MesaId == mesaId &&
                        p.Id != pedido.Id &&
                        (p.Estado == PedidoEstado.Pendiente || p.Estado == PedidoEstado.EnPreparacion));

                    if (!tieneOtrosPedidosActivos)
                    {
                        var mesa = await _context.Mesas.FindAsync(mesaId);
                        if (mesa != null)
                        {
                            mesa.Estado = MesaEstado.Disponible;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = $"Estado del pedido #{pedido.Id} actualizado a '{dto.NuevoEstado}' exitosamente." });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
