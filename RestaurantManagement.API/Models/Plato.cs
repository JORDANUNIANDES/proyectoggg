using System;
using System.Collections.Generic;

namespace RestaurantManagement.API.Models
{
    public class Plato
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public bool Disponible { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Relaciones
        public ICollection<DetallePedido> DetallesPedido { get; set; } = new List<DetallePedido>();
    }
}
