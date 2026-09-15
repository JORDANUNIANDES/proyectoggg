using System.Collections.Generic;

namespace RestaurantManagement.API.Models
{
    public class Mesa
    {
        public int Id { get; set; }
        public int NumeroMesa { get; set; }
        public int Capacidad { get; set; }
        public MesaEstado Estado { get; set; } = MesaEstado.Disponible;
        public bool Activo { get; set; } = true;

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
