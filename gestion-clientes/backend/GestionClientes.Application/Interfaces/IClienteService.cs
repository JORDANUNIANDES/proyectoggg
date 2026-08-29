using GestionClientes.Application.DTOs;

namespace GestionClientes.Application.Interfaces;

public interface IClienteService
{
    Task<IEnumerable<ClienteDto>> ObtenerTodosAsync(string? busqueda = null, bool? activo = null);
    Task<ClienteDto?> ObtenerPorIdAsync(int id);
    Task<ClienteDto> CrearAsync(CrearClienteDto dto);
    Task<ClienteDto> ActualizarAsync(int id, ActualizarClienteDto dto);
    Task<bool> CambiarEstadoAsync(int id, bool activo);
    Task<DashboardSummaryDto> ObtenerResumenDashboardAsync();
}
