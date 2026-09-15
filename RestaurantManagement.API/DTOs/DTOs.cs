using System;
using System.Collections.Generic;
using RestaurantManagement.API.Models;

namespace RestaurantManagement.API.DTOs
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class ClienteCreateUpdateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }

    public class PlatoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public bool Disponible { get; set; }
        public bool Activo { get; set; }
    }

    public class PlatoCreateUpdateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public bool Disponible { get; set; } = true;
        public bool Activo { get; set; } = true;
    }

    public class MesaDto
    {
        public int Id { get; set; }
        public int NumeroMesa { get; set; }
        public int Capacidad { get; set; }
        public MesaEstado Estado { get; set; }
        public string EstadoNombre => Estado.ToString();
        public bool Activo { get; set; }
    }

    public class MesaCreateUpdateDto
    {
        public int NumeroMesa { get; set; }
        public int Capacidad { get; set; }
        public MesaEstado Estado { get; set; } = MesaEstado.Disponible;
        public bool Activo { get; set; } = true;
    }

    public class DetallePedidoCreateDto
    {
        public int PlatoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class PedidoCreateDto
    {
        public int ClienteId { get; set; }
        public int? MesaId { get; set; }
        public List<DetallePedidoCreateDto> Detalles { get; set; } = new();
    }

    public class DetallePedidoDto
    {
        public int Id { get; set; }
        public int PlatoId { get; set; }
        public string PlatoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class PedidoDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombreCompleto { get; set; } = string.Empty;
        public string ClienteCedula { get; set; } = string.Empty;
        public int? MesaId { get; set; }
        public int? NumeroMesa { get; set; }
        public DateTime Fecha { get; set; }
        public PedidoEstado Estado { get; set; }
        public string EstadoNombre => Estado.ToString();
        public decimal Total { get; set; }
        public List<DetallePedidoDto> Detalles { get; set; } = new();
    }

    public class PedidoEstadoUpdateDto
    {
        public PedidoEstado NuevoEstado { get; set; }
    }

    public class ReporteFiltroDto
    {
        public int ClienteId { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public PedidoEstado? Estado { get; set; }
    }

    public class ReporteClienteDto
    {
        public ClienteDto Cliente { get; set; } = null!;
        public List<PedidoDto> Pedidos { get; set; } = new();
        public decimal TotalVentas { get; set; }
        public int TotalPedidos { get; set; }
    }

    public class DashboardSummaryDto
    {
        public int TotalClientes { get; set; }
        public int TotalPlatos { get; set; }
        public int MesasDisponibles { get; set; }
        public int MesasOcupadas { get; set; }
        public int PedidosHoy { get; set; }
        public decimal VentasHoy { get; set; }
    }
}
