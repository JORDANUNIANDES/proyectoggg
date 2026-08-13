using System;

namespace MuscleHouse.Models
{
    public class Asistencia
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public DateTime FechaHora { get; set; }
    }
}
