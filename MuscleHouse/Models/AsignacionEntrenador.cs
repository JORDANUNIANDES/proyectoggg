using System;

namespace MuscleHouse.Models
{
    public class AsignacionEntrenador
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public int EntrenadorId { get; set; }
        public Entrenador? Entrenador { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
