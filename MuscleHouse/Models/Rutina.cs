using System;
using System.Collections.Generic;

namespace MuscleHouse.Models
{
    public class Rutina
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public int EntrenadorId { get; set; }
        public Entrenador? Entrenador { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public bool Activa { get; set; }

        // Navigation properties
        public ICollection<RutinaEjercicio> RutinaEjercicios { get; set; } = new List<RutinaEjercicio>();
    }
}
