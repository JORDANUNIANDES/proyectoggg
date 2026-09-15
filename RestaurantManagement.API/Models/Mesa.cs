using System;
using System.Collections.Generic;

namespace RestaurantManagement.API.Models
{
    public class Mesa
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public int Capacidad { get; set; }
        public string Ubicacion { get; set; } = string.Empty;
        public string Estado { get; set; } = "Disponible"; // Disponible, Ocupada, Reservada, Mantenimiento

        // Relaciones
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
