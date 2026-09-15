using System;
using System.Collections.Generic;

namespace RestaurantManagement.API.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public int? MesaId { get; set; }
        public Mesa? Mesa { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;
        public PedidoEstado Estado { get; set; } = PedidoEstado.Pendiente;
        public decimal Total { get; set; }

        public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }
}
