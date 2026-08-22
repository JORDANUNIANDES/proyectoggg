using System;

namespace MuscleHouse.Models
{
    public class RegistroEntrenamiento
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public int EjercicioId { get; set; }
        public Ejercicio? Ejercicio { get; set; }

        public DateTime Fecha { get; set; }
        public int Series { get; set; }
        public int Repeticiones { get; set; }
        public decimal Peso { get; set; }
        public int? RPE { get; set; } // 1 a 10
        public string Observaciones { get; set; } = string.Empty;
    }
}
