using System.Collections.Generic;

namespace MuscleHouse.Models
{
    public class Entrenador
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public int Experiencia { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Fotografia { get; set; } = string.Empty;
        public bool Activo { get; set; }

        // Navigation properties
        public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
        public ICollection<AsignacionEntrenador> AsignacionesClientes { get; set; } = new List<AsignacionEntrenador>();
        public ICollection<Rutina> Rutinas { get; set; } = new List<Rutina>();
    }
}
