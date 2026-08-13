using System.Collections.Generic;
using MuscleHouse.Models;

namespace MuscleHouse.ViewModels
{
    public class ClienteDashboardViewModel
    {
        public Cliente? Cliente { get; set; }
        public Membresia? MembresiaActual { get; set; }
        public int DiasRestantes { get; set; }
        public Entrenador? Entrenador { get; set; }
        public Rutina? RutinaActiva { get; set; }

        public List<Asistencia> UltimasAsistencias { get; set; } = new List<Asistencia>();
        public List<Progreso> HistorialProgreso { get; set; } = new List<Progreso>();
        public List<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
    }
}
