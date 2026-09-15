using System;
using System.Collections.Generic;

namespace RestaurantManagement.API.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public int? MesaId { get; set; }
        public Mesa? Mesa { get; set; }

        public DateTime FechaPedido { get; set; } = DateTime.Now;
        public string Estado { get; set; } = "Pendiente"; // Pendiente, En preparación, Servido, Pagado, Cancelado
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public string? Observaciones { get; set; }

        // Relaciones
        public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }
}
