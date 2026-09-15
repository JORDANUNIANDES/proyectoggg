using System;
using System.Collections.Generic;

namespace RestaurantManagement.API.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public bool Estado { get; set; } = true;

        // Relaciones
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
