using System;

namespace MuscleHouse.Models
{
    public class Progreso
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public DateTime Fecha { get; set; }
        public decimal Peso { get; set; }
        public decimal Pecho { get; set; }
        public decimal Cintura { get; set; }
        public decimal Brazo { get; set; }
        public decimal Pierna { get; set; }
        public decimal Cadera { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }
}
