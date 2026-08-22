using System;

namespace MuscleHouse.Models
{
    public class Pago
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public int MembresiaId { get; set; }
        public Membresia? Membresia { get; set; }

        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string MetodoPago { get; set; } = string.Empty; // Efectivo, Tarjeta, Transferencia
        public string Estado { get; set; } = string.Empty; // Completado, Pendiente, Cancelado
    }
}
