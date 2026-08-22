namespace MuscleHouse.Models
{
    public class RutinaEjercicio
    {
        public int Id { get; set; }
        public int RutinaId { get; set; }
        public Rutina? Rutina { get; set; }

        public int EjercicioId { get; set; }
        public Ejercicio? Ejercicio { get; set; }

        public int Series { get; set; }
        public int Repeticiones { get; set; }
        public decimal PesoRecomendado { get; set; }
        public int DescansoSegundos { get; set; }
        public int Orden { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }
}
