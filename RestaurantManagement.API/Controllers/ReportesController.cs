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
    public class ReportesController : ControllerBase
    {
        private readonly IRestaurantService _service;

        public ReportesController(IRestaurantService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene el reporte detallado de pedidos por cliente.
        /// </summary>
        [HttpGet("pedidos-por-cliente/{clienteId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReportePedidoClienteDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReportePedidoClienteDto>> GetReportePedidosPorCliente(int clienteId)
        {
            var reporte = await _service.GetReportePedidosPorClienteAsync(clienteId);
            if (reporte == null)
            {
                return NotFound(new { message = $"No se encontró el cliente con ID {clienteId}." });
            }
            return Ok(reporte);
        }

        /// <summary>
        /// Obtiene métricas generales del dashboard del restaurante.
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DashboardSummaryDto))]
        public async Task<ActionResult<DashboardSummaryDto>> GetDashboard()
        {
            var dashboard = await _service.GetDashboardSummaryAsync();
            return Ok(dashboard);
        }
    }
}
