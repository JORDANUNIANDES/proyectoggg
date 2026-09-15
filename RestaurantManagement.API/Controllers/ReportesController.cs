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
    public class ReportesController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public ReportesController(RestaurantDbContext context)
        {
            _context = context;
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<ReporteClienteDto>> GetReporteCliente(
            int clienteId,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] PedidoEstado? estado)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
                return NotFound(new { mensaje = "Cliente no encontrado." });

            var query = _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Mesa)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .Where(p => p.ClienteId == clienteId)
                .AsQueryable();

            if (fechaInicio.HasValue)
            {
                var inicio = fechaInicio.Value.Date;
                query = query.Where(p => p.Fecha >= inicio);
            }

            if (fechaFin.HasValue)
            {
                var fin = fechaFin.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.Fecha <= fin);
            }

            if (estado.HasValue)
            {
                query = query.Where(p => p.Estado == estado.Value);
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

            decimal totalVentas = pedidos.Where(p => p.Estado != PedidoEstado.Cancelado).Sum(p => p.Total);

            var reporte = new ReporteClienteDto
            {
                Cliente = new ClienteDto
                {
                    Id = cliente.Id,
                    Nombre = cliente.Nombre,
                    Apellido = cliente.Apellido,
                    Cedula = cliente.Cedula,
                    Telefono = cliente.Telefono,
                    Email = cliente.Email,
                    Activo = cliente.Activo
                },
                Pedidos = pedidos,
                TotalVentas = totalVentas,
                TotalPedidos = pedidos.Count
            };

            return Ok(reporte);
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary()
        {
            var hoy = DateTime.Today;
            var finHoy = hoy.AddDays(1).AddTicks(-1);

            var totalClientes = await _context.Clientes.CountAsync(c => c.Activo);
            var totalPlatos = await _context.Platos.CountAsync(p => p.Activo);
            var mesasDisponibles = await _context.Mesas.CountAsync(m => m.Activo && m.Estado == MesaEstado.Disponible);
            var mesasOcupadas = await _context.Mesas.CountAsync(m => m.Activo && m.Estado == MesaEstado.Ocupada);

            var pedidosHoyQuery = _context.Pedidos.Where(p => p.Fecha >= hoy && p.Fecha <= finHoy);
            var pedidosHoyCount = await pedidosHoyQuery.CountAsync();
            var ventasHoy = await pedidosHoyQuery
                .Where(p => p.Estado != PedidoEstado.Cancelado)
                .SumAsync(p => (decimal?)p.Total) ?? 0m;

            var summary = new DashboardSummaryDto
            {
                TotalClientes = totalClientes,
                TotalPlatos = totalPlatos,
                MesasDisponibles = mesasDisponibles,
                MesasOcupadas = mesasOcupadas,
                PedidosHoy = pedidosHoyCount,
                VentasHoy = ventasHoy
            };

            return Ok(summary);
        }
    }
}
