using GestionClientes.Application.DTOs;
using GestionClientes.Application.Interfaces;
using GestionClientes.Application.Validators;
using GestionClientes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionClientes.Application.Services;

public class ClienteService : IClienteService
{
    private readonly DbContext _context;
    private readonly DbSet<Cliente> _clientes;

    public ClienteService(DbContext context)
    {
        _context = context;
        _clientes = _context.Set<Cliente>();
    }

    public async Task<IEnumerable<ClienteDto>> ObtenerTodosAsync(string? busqueda = null, bool? activo = null)
    {
        IQueryable<Cliente> query = _clientes.AsNoTracking();

        if (activo.HasValue)
        {
            query = query.Where(c => c.Activo == activo.Value);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var term = busqueda.Trim().ToLower();
            query = query.Where(c =>
                c.Nombres.ToLower().Contains(term) ||
                c.Apellidos.ToLower().Contains(term) ||
                c.Ruc.Contains(term) ||
                c.Email.ToLower().Contains(term) ||
                c.Telefono.Contains(term) ||
                c.Direccion.ToLower().Contains(term)
            );
        }

        var clientes = await query.OrderByDescending(c => c.FechaRegistro).ToListAsync();
        return clientes.Select(MapToDto);
    }

    public async Task<ClienteDto?> ObtenerPorIdAsync(int id)
    {
        var cliente = await _clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return cliente == null ? null : MapToDto(cliente);
    }

    public async Task<ClienteDto> CrearAsync(CrearClienteDto dto)
    {
        ValidarCamposCliente(dto.Nombres, dto.Apellidos, dto.Ruc, dto.Email, dto.Direccion);

        var rucExiste = await _clientes.AnyAsync(c => c.Ruc == dto.Ruc.Trim());
        if (rucExiste)
        {
            throw new InvalidOperationException("Este RUC ya está registrado.");
        }

        var cliente = new Cliente
        {
            Nombres = dto.Nombres.Trim(),
            Apellidos = dto.Apellidos.Trim(),
            Ruc = dto.Ruc.Trim(),
            Email = dto.Email.Trim().ToLower(),
            Telefono = dto.Telefono?.Trim() ?? string.Empty,
            Direccion = dto.Direccion.Trim(),
            FechaRegistro = DateTime.UtcNow,
            Activo = true
        };

        _clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return MapToDto(cliente);
    }

    public async Task<ClienteDto> ActualizarAsync(int id, ActualizarClienteDto dto)
    {
        ValidarCamposCliente(dto.Nombres, dto.Apellidos, dto.Ruc, dto.Email, dto.Direccion);

        var cliente = await _clientes.FindAsync(id);
        if (cliente == null)
        {
            throw new KeyNotFoundException("Cliente no encontrado.");
        }

        var rucExiste = await _clientes.AnyAsync(c => c.Ruc == dto.Ruc.Trim() && c.Id != id);
        if (rucExiste)
        {
            throw new InvalidOperationException("Este RUC ya está registrado.");
        }

        cliente.Nombres = dto.Nombres.Trim();
        cliente.Apellidos = dto.Apellidos.Trim();
        cliente.Ruc = dto.Ruc.Trim();
        cliente.Email = dto.Email.Trim().ToLower();
        cliente.Telefono = dto.Telefono?.Trim() ?? string.Empty;
        cliente.Direccion = dto.Direccion.Trim();

        await _context.SaveChangesAsync();

        return MapToDto(cliente);
    }

    public async Task<bool> CambiarEstadoAsync(int id, bool activo)
    {
        var cliente = await _clientes.FindAsync(id);
        if (cliente == null)
        {
            return false;
        }

        cliente.Activo = activo;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<DashboardSummaryDto> ObtenerResumenDashboardAsync()
    {
        var total = await _clientes.CountAsync();
        var activos = await _clientes.CountAsync(c => c.Activo);
        var inactivos = await _clientes.CountAsync(c => !c.Activo);

        var haceTreintaDias = DateTime.UtcNow.AddDays(-30);
        var recientes = await _clientes.CountAsync(c => c.FechaRegistro >= haceTreintaDias);

        var ultimos = await _clientes.AsNoTracking()
            .OrderByDescending(c => c.FechaRegistro)
            .Take(5)
            .ToListAsync();

        return new DashboardSummaryDto
        {
            TotalClientes = total,
            ClientesActivos = activos,
            ClientesInactivos = inactivos,
            ClientesRecientes = recientes,
            UltimosClientes = ultimos.Select(MapToDto).ToList()
        };
    }

    private static void ValidarCamposCliente(string nombres, string apellidos, string ruc, string email, string direccion)
    {
        if (string.IsNullOrWhiteSpace(nombres))
            throw new ArgumentException("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(apellidos))
            throw new ArgumentException("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(ruc))
            throw new ArgumentException("El RUC es obligatorio.");

        if (!RucEcuadorValidator.EsRucValido(ruc))
            throw new ArgumentException("El RUC ingresado no es válido para Ecuador.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El correo electrónico es obligatorio.");

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email.Trim())
                throw new ArgumentException("El correo electrónico no es válido.");
        }
        catch
        {
            throw new ArgumentException("El correo electrónico no es válido.");
        }

        if (string.IsNullOrWhiteSpace(direccion))
            throw new ArgumentException("La dirección es obligatoria.");
    }

    private static ClienteDto MapToDto(Cliente cliente)
    {
        return new ClienteDto
        {
            Id = cliente.Id,
            Nombres = cliente.Nombres,
            Apellidos = cliente.Apellidos,
            Ruc = cliente.Ruc,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            Direccion = cliente.Direccion,
            FechaRegistro = cliente.FechaRegistro,
            Activo = cliente.Activo
        };
    }
}
