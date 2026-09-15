using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.API.Data;
using RestaurantManagement.API.DTOs;
using RestaurantManagement.API.Models;

namespace RestaurantManagement.API.Services
{
    public interface IRestaurantService
    {
        // Clientes
        Task<IEnumerable<ClienteDto>> GetClientesAsync();
        Task<ClienteDto?> GetClienteByIdAsync(int id);
        Task<(ClienteDto? Result, string? Error)> CreateClienteAsync(ClienteCreateDto dto);
        Task<(bool Success, string? Error)> UpdateClienteAsync(int id, ClienteUpdateDto dto);
        Task<(bool Success, string? Error)> DeleteClienteAsync(int id);

        // Platos
        Task<IEnumerable<PlatoDto>> GetPlatosAsync(bool? disponible = null, string? categoria = null);
        Task<PlatoDto?> GetPlatoByIdAsync(int id);
        Task<(PlatoDto? Result, string? Error)> CreatePlatoAsync(PlatoCreateDto dto);
        Task<(bool Success, string? Error)> UpdatePlatoAsync(int id, PlatoUpdateDto dto);
        Task<(bool Success, string? Error)> DeletePlatoAsync(int id);

        // Mesas
        Task<IEnumerable<MesaDto>> GetMesasAsync();
        Task<MesaDto?> GetMesaByIdAsync(int id);
        Task<(MesaDto? Result, string? Error)> CreateMesaAsync(MesaCreateDto dto);
        Task<(bool Success, string? Error)> UpdateMesaAsync(int id, MesaUpdateDto dto);
        Task<(bool Success, string? Error)> DeleteMesaAsync(int id);

        // Pedidos
        Task<IEnumerable<PedidoDto>> GetPedidosAsync(int? clienteId = null);
        Task<PedidoDto?> GetPedidoByIdAsync(int id);
        Task<(PedidoDto? Result, string? Error)> CreatePedidoAsync(PedidoCreateDto dto);
        Task<(bool Success, string? Error)> UpdateEstadoPedidoAsync(int id, string nuevoEstado);
        Task<(bool Success, string? Error)> CancelarPedidoAsync(int id);

        // Reportes & Dashboard
        Task<ReportePedidoClienteDto?> GetReportePedidosPorClienteAsync(int clienteId);
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    }

    public class RestaurantService : IRestaurantService
    {
        private readonly ApplicationDbContext _context;

        public RestaurantService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Clientes

        public async Task<IEnumerable<ClienteDto>> GetClientesAsync()
        {
            return await _context.Clientes
                .Select(c => MapClienteToDto(c))
                .ToListAsync();
        }

        public async Task<ClienteDto?> GetClienteByIdAsync(int id)
        {
            var c = await _context.Clientes.FindAsync(id);
            return c == null ? null : MapClienteToDto(c);
        }

        public async Task<(ClienteDto? Result, string? Error)> CreateClienteAsync(ClienteCreateDto dto)
        {
            if (!EcuadorianValidator.ValidarCedula(dto.Cedula))
            {
                return (null, "La cédula ingresada no es una cédula ecuatoriana válida.");
            }

            bool existsCedula = await _context.Clientes.AnyAsync(c => c.Cedula == dto.Cedula);
            if (existsCedula)
            {
                return (null, "Ya existe un cliente registrado con la misma cédula.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                bool existsEmail = await _context.Clientes.AnyAsync(c => c.Email == dto.Email);
                if (existsEmail)
                {
                    return (null, "Ya existe un cliente registrado con el mismo correo electrónico.");
                }
            }

            var cliente = new Cliente
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Cedula = dto.Cedula,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Direccion = dto.Direccion,
                FechaRegistro = DateTime.Now,
                Estado = true
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return (MapClienteToDto(cliente), null);
        }

        public async Task<(bool Success, string? Error)> UpdateClienteAsync(int id, ClienteUpdateDto dto)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return (false, "Cliente no encontrado.");
            }

            if (!EcuadorianValidator.ValidarCedula(dto.Cedula))
            {
                return (false, "La cédula ingresada no es una cédula ecuatoriana válida.");
            }

            bool existsCedula = await _context.Clientes.AnyAsync(c => c.Cedula == dto.Cedula && c.Id != id);
            if (existsCedula)
            {
                return (false, "Ya existe otro cliente registrado con la misma cédula.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                bool existsEmail = await _context.Clientes.AnyAsync(c => c.Email == dto.Email && c.Id != id);
                if (existsEmail)
                {
                    return (false, "Ya existe otro cliente registrado con el mismo correo electrónico.");
                }
            }

            cliente.Nombre = dto.Nombre;
            cliente.Apellido = dto.Apellido;
            cliente.Cedula = dto.Cedula;
            cliente.Telefono = dto.Telefono;
            cliente.Email = dto.Email;
            cliente.Direccion = dto.Direccion;
            cliente.Estado = dto.Estado;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteClienteAsync(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return (false, "Cliente no encontrado.");
            }

            bool tienePedidos = await _context.Pedidos.AnyAsync(p => p.ClienteId == id);
            if (tienePedidos)
            {
                return (false, "No se puede eliminar el cliente porque tiene pedidos registrados.");
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        private static ClienteDto MapClienteToDto(Cliente c) => new ClienteDto
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Apellido = c.Apellido,
            Cedula = c.Cedula,
            Telefono = c.Telefono,
            Email = c.Email,
            Direccion = c.Direccion,
            FechaRegistro = c.FechaRegistro,
            Estado = c.Estado
        };

        #endregion

        #region Platos

        public async Task<IEnumerable<PlatoDto>> GetPlatosAsync(bool? disponible = null, string? categoria = null)
        {
            var query = _context.Platos.AsQueryable();

            if (disponible.HasValue)
            {
                query = query.Where(p => p.Disponible == disponible.Value);
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                query = query.Where(p => p.Categoria.ToLower() == categoria.ToLower());
            }

            return await query.Select(p => MapPlatoToDto(p)).ToListAsync();
        }

        public async Task<PlatoDto?> GetPlatoByIdAsync(int id)
        {
            var plato = await _context.Platos.FindAsync(id);
            return plato == null ? null : MapPlatoToDto(plato);
        }

        public async Task<(PlatoDto? Result, string? Error)> CreatePlatoAsync(PlatoCreateDto dto)
        {
            if (dto.Precio <= 0)
            {
                return (null, "El precio del plato debe ser mayor a 0.");
            }

            bool nombreExiste = await _context.Platos.AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower());
            if (nombreExiste)
            {
                return (null, "Ya existe un plato registrado con ese nombre.");
            }

            var plato = new Plato
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                Categoria = dto.Categoria,
                Disponible = dto.Disponible,
                FechaRegistro = DateTime.Now
            };

            _context.Platos.Add(plato);
            await _context.SaveChangesAsync();

            return (MapPlatoToDto(plato), null);
        }

        public async Task<(bool Success, string? Error)> UpdatePlatoAsync(int id, PlatoUpdateDto dto)
        {
            var plato = await _context.Platos.FindAsync(id);
            if (plato == null)
            {
                return (false, "Plato no encontrado.");
            }

            if (dto.Precio <= 0)
            {
                return (false, "El precio del plato debe ser mayor a 0.");
            }

            bool nombreExiste = await _context.Platos.AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower() && p.Id != id);
            if (nombreExiste)
            {
                return (false, "Ya existe otro plato registrado con ese nombre.");
            }

            plato.Nombre = dto.Nombre;
            plato.Descripcion = dto.Descripcion;
            plato.Precio = dto.Precio;
            plato.Categoria = dto.Categoria;
            plato.Disponible = dto.Disponible;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeletePlatoAsync(int id)
        {
            var plato = await _context.Platos.FindAsync(id);
            if (plato == null)
            {
                return (false, "Plato no encontrado.");
            }

            bool tieneDetalles = await _context.DetallesPedido.AnyAsync(d => d.PlatoId == id);
            if (tieneDetalles)
            {
                // Regla de negocio: Baja lógica mediante Disponible = false si ya está en historial de pedidos
                plato.Disponible = false;
                await _context.SaveChangesAsync();
                return (true, "El plato no puede ser eliminado físicamente porque forma parte del historial de pedidos. Ha sido marcado como no disponible.");
            }

            _context.Platos.Remove(plato);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        private static PlatoDto MapPlatoToDto(Plato p) => new PlatoDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            Precio = p.Precio,
            Categoria = p.Categoria,
            Disponible = p.Disponible,
            FechaRegistro = p.FechaRegistro
        };

        #endregion

        #region Mesas

        public async Task<IEnumerable<MesaDto>> GetMesasAsync()
        {
            return await _context.Mesas
                .Select(m => MapMesaToDto(m))
                .ToListAsync();
        }

        public async Task<MesaDto?> GetMesaByIdAsync(int id)
        {
            var mesa = await _context.Mesas.FindAsync(id);
            return mesa == null ? null : MapMesaToDto(mesa);
        }

        public async Task<(MesaDto? Result, string? Error)> CreateMesaAsync(MesaCreateDto dto)
        {
            if (dto.Capacidad <= 0)
            {
                return (null, "La capacidad de la mesa debe ser mayor a 0.");
            }

            bool numeroExiste = await _context.Mesas.AnyAsync(m => m.Numero == dto.Numero);
            if (numeroExiste)
            {
                return (null, "Ya existe una mesa con ese número.");
            }

            var mesa = new Mesa
            {
                Numero = dto.Numero,
                Capacidad = dto.Capacidad,
                Ubicacion = dto.Ubicacion,
                Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Disponible" : dto.Estado
            };

            _context.Mesas.Add(mesa);
            await _context.SaveChangesAsync();

            return (MapMesaToDto(mesa), null);
        }

        public async Task<(bool Success, string? Error)> UpdateMesaAsync(int id, MesaUpdateDto dto)
        {
            var mesa = await _context.Mesas.FindAsync(id);
            if (mesa == null)
            {
                return (false, "Mesa no encontrada.");
            }

            if (dto.Capacidad <= 0)
            {
                return (false, "La capacidad de la mesa debe ser mayor a 0.");
            }

            bool numeroExiste = await _context.Mesas.AnyAsync(m => m.Numero == dto.Numero && m.Id != id);
            if (numeroExiste)
            {
                return (false, "Ya existe otra mesa registrada con ese número.");
            }

            mesa.Numero = dto.Numero;
            mesa.Capacidad = dto.Capacidad;
            mesa.Ubicacion = dto.Ubicacion;
            mesa.Estado = dto.Estado;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteMesaAsync(int id)
        {
            var mesa = await _context.Mesas.FindAsync(id);
            if (mesa == null)
            {
                return (false, "Mesa no encontrada.");
            }

            bool tienePedidos = await _context.Pedidos.AnyAsync(p => p.MesaId == id);
            if (tienePedidos)
            {
                return (false, "No se puede eliminar la mesa porque tiene pedidos registrados.");
            }

            _context.Mesas.Remove(mesa);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        private static MesaDto MapMesaToDto(Mesa m) => new MesaDto
        {
            Id = m.Id,
            Numero = m.Numero,
            Capacidad = m.Capacidad,
            Ubicacion = m.Ubicacion,
            Estado = m.Estado
        };

        #endregion

        #region Pedidos

        public async Task<IEnumerable<PedidoDto>> GetPedidosAsync(int? clienteId = null)
        {
            var query = _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Mesa)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .AsQueryable();

            if (clienteId.HasValue)
            {
                query = query.Where(p => p.ClienteId == clienteId.Value);
            }

            var pedidos = await query.OrderByDescending(p => p.FechaPedido).ToListAsync();
            return pedidos.Select(p => MapPedidoToDto(p));
        }

        public async Task<PedidoDto?> GetPedidoByIdAsync(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Mesa)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .FirstOrDefaultAsync(p => p.Id == id);

            return pedido == null ? null : MapPedidoToDto(pedido);
        }

        public async Task<(PedidoDto? Result, string? Error)> CreatePedidoAsync(PedidoCreateDto dto)
        {
            if (dto.Detalles == null || !dto.Detalles.Any())
            {
                return (null, "El pedido debe incluir al menos un plato.");
            }

            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null || !cliente.Estado)
            {
                return (null, "El cliente seleccionado no existe o está inactivo.");
            }

            Mesa? mesa = null;
            if (dto.MesaId.HasValue)
            {
                mesa = await _context.Mesas.FindAsync(dto.MesaId.Value);
                if (mesa == null)
                {
                    return (null, "La mesa seleccionada no existe.");
                }
                if (mesa.Estado == "Mantenimiento")
                {
                    return (null, "La mesa seleccionada está en mantenimiento y no se puede usar.");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var nuevoPedido = new Pedido
                {
                    ClienteId = dto.ClienteId,
                    MesaId = dto.MesaId,
                    FechaPedido = DateTime.Now,
                    Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Pendiente" : dto.Estado,
                    Observaciones = dto.Observaciones,
                    Subtotal = 0,
                    Total = 0
                };

                decimal subtotalGeneral = 0;

                foreach (var item in dto.Detalles)
                {
                    if (item.Cantidad <= 0)
                    {
                        return (null, "La cantidad de cada plato debe ser mayor que cero.");
                    }

                    var plato = await _context.Platos.FindAsync(item.PlatoId);
                    if (plato == null)
                    {
                        return (null, $"El plato con ID {item.PlatoId} no existe.");
                    }
                    if (!plato.Disponible)
                    {
                        return (null, $"El plato '{plato.Nombre}' no está disponible actualmente.");
                    }

                    decimal subtotalDetalle = plato.Precio * item.Cantidad;
                    subtotalGeneral += subtotalDetalle;

                    nuevoPedido.Detalles.Add(new DetallePedido
                    {
                        PlatoId = plato.Id,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = plato.Precio,
                        Subtotal = subtotalDetalle
                    });
                }

                nuevoPedido.Subtotal = subtotalGeneral;
                nuevoPedido.Total = subtotalGeneral;

                if (mesa != null && nuevoPedido.Estado != "Pagado" && nuevoPedido.Estado != "Cancelado")
                {
                    mesa.Estado = "Ocupada";
                }

                _context.Pedidos.Add(nuevoPedido);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (await GetPedidoByIdAsync(nuevoPedido.Id), null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (null, $"Error al registrar el pedido: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? Error)> UpdateEstadoPedidoAsync(int id, string nuevoEstado)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Mesa)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                return (false, "Pedido no encontrado.");
            }

            pedido.Estado = nuevoEstado;

            if (pedido.Mesa != null)
            {
                if (nuevoEstado == "Pagado" || nuevoEstado == "Cancelado")
                {
                    pedido.Mesa.Estado = "Disponible";
                }
                else if (nuevoEstado == "Pendiente" || nuevoEstado == "En preparación" || nuevoEstado == "Servido")
                {
                    pedido.Mesa.Estado = "Ocupada";
                }
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> CancelarPedidoAsync(int id)
        {
            return await UpdateEstadoPedidoAsync(id, "Cancelado");
        }

        private static PedidoDto MapPedidoToDto(Pedido p) => new PedidoDto
        {
            Id = p.Id,
            ClienteId = p.ClienteId,
            ClienteNombreCompleto = p.Cliente != null ? $"{p.Cliente.Nombre} {p.Cliente.Apellido}" : string.Empty,
            ClienteCedula = p.Cliente != null ? p.Cliente.Cedula : string.Empty,
            MesaId = p.MesaId,
            MesaNumero = p.Mesa != null ? p.Mesa.Numero : (int?)null,
            FechaPedido = p.FechaPedido,
            Estado = p.Estado,
            Subtotal = p.Subtotal,
            Total = p.Total,
            Observaciones = p.Observaciones,
            Detalles = p.Detalles.Select(d => new DetallePedidoDto
            {
                Id = d.Id,
                PlatoId = d.PlatoId,
                PlatoNombre = d.Plato != null ? d.Plato.Nombre : string.Empty,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal
            }).ToList()
        };

        #endregion

        #region Reportes & Dashboard

        public async Task<ReportePedidoClienteDto?> GetReportePedidosPorClienteAsync(int clienteId)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                return null;
            }

            var pedidos = await GetPedidosAsync(clienteId);
            var listaPedidos = pedidos.ToList();

            return new ReportePedidoClienteDto
            {
                ClienteId = cliente.Id,
                NombreCliente = cliente.Nombre,
                ApellidoCliente = cliente.Apellido,
                CedulaCliente = cliente.Cedula,
                TotalGastado = listaPedidos.Where(p => p.Estado != "Cancelado").Sum(p => p.Total),
                CantidadPedidos = listaPedidos.Count,
                Pedidos = listaPedidos
            };
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            return new DashboardSummaryDto
            {
                TotalClientes = await _context.Clientes.CountAsync(c => c.Estado),
                PlatosDisponibles = await _context.Platos.CountAsync(p => p.Disponible),
                TotalMesas = await _context.Mesas.CountAsync(),
                PedidosRegistrados = await _context.Pedidos.CountAsync(),
                PedidosPendientes = await _context.Pedidos.CountAsync(p => p.Estado == "Pendiente" || p.Estado == "En preparación"),
                VentasTotales = await _context.Pedidos.Where(p => p.Estado == "Pagado").SumAsync(p => p.Total)
            };
        }

        #endregion
    }
}
