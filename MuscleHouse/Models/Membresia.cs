using System;
using System.Collections.Generic;

namespace MuscleHouse.Models
{
    public class Membresia
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public int PlanId { get; set; }
        public Plan? Plan { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal PrecioPagado { get; set; }
        public string Estado { get; set; } = "Activa"; // Activa, Vencida, Cancelada

        // Navigation properties
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}
