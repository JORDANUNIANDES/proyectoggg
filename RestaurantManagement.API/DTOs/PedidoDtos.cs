using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.API.DTOs
{
    public class PedidoDetalleCreateDto
    {
        [Required]
        public int PlatoId { get; set; }

        [Range(1, 100, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }
    }

    public class PedidoCreateDto
    {
        [Required(ErrorMessage = "El cliente es obligatorio.")]
        public int ClienteId { get; set; }

        public int? MesaId { get; set; }

        public string? Observaciones { get; set; }

        public string Estado { get; set; } = "Pendiente";

        [Required]
        [MinLength(1, ErrorMessage = "Debe agregar al menos un plato al pedido.")]
        public List<PedidoDetalleCreateDto> Detalles { get; set; } = new List<PedidoDetalleCreateDto>();
    }

    public class PedidoUpdateEstadoDto
    {
        [Required]
        public string Estado { get; set; } = string.Empty;
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
        public int? MesaNumero { get; set; }

        public DateTime FechaPedido { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public string? Observaciones { get; set; }

        public List<DetallePedidoDto> Detalles { get; set; } = new List<DetallePedidoDto>();
    }

    public class ReportePedidoClienteDto
    {
        public int ClienteId { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string ApellidoCliente { get; set; } = string.Empty;
        public string CedulaCliente { get; set; } = string.Empty;
        public decimal TotalGastado { get; set; }
        public int CantidadPedidos { get; set; }
        public List<PedidoDto> Pedidos { get; set; } = new List<PedidoDto>();
    }

    public class DashboardSummaryDto
    {
        public int TotalClientes { get; set; }
        public int PlatosDisponibles { get; set; }
        public int TotalMesas { get; set; }
        public int PedidosRegistrados { get; set; }
        public int PedidosPendientes { get; set; }
        public decimal VentasTotales { get; set; }
    }
}
