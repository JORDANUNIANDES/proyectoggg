using System.Collections.Generic;

namespace MuscleHouse.Models
{
    public class Ejercicio
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string GrupoMuscular { get; set; } = string.Empty;
        public string Instrucciones { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;

        // Navigation properties
        public ICollection<RutinaEjercicio> RutinasEjercicios { get; set; } = new List<RutinaEjercicio>();
        public ICollection<RegistroEntrenamiento> RegistrosEntrenamiento { get; set; } = new List<RegistroEntrenamiento>();
    }
}
