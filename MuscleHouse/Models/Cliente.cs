using System;
using System.Collections.Generic;

namespace MuscleHouse.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Objetivo { get; set; } = string.Empty;

        public int? EntrenadorId { get; set; }
        public Entrenador? Entrenador { get; set; }

        public bool Activo { get; set; }

        // Navigation properties
        public ICollection<AsignacionEntrenador> AsignacionesEntrenadores { get; set; } = new List<AsignacionEntrenador>();
        public ICollection<Membresia> Membresias { get; set; } = new List<Membresia>();
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
        public ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();
        public ICollection<Progreso> Progresos { get; set; } = new List<Progreso>();
        public ICollection<Rutina> Rutinas { get; set; } = new List<Rutina>();
        public ICollection<RegistroEntrenamiento> RegistrosEntrenamiento { get; set; } = new List<RegistroEntrenamiento>();
    }
}
