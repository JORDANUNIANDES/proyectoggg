using System.Collections.Generic;

namespace MuscleHouse.Models
{
    public class Plan
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty; // Diario, Mensual, Trimestral, Semestral, Anual
        public int DuracionDias { get; set; }
        public decimal Precio { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;

        // Navigation properties
        public ICollection<Membresia> Membresias { get; set; } = new List<Membresia>();
    }
}
