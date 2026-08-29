using GestionClientes.Application.DTOs;
using GestionClientes.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GestionClientes.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public DashboardController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryDto>> ObtenerResumen()
    {
        var resumen = await _clienteService.ObtenerResumenDashboardAsync();
        return Ok(resumen);
    }
}
