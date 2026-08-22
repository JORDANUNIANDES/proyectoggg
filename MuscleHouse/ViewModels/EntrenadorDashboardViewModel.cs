using System.Collections.Generic;
using MuscleHouse.Models;

namespace MuscleHouse.ViewModels
{
    public class EntrenadorDashboardViewModel
    {
        public int TotalClientesAsignados { get; set; }
        public int TotalRutinasActivas { get; set; }
        public List<Cliente> ClientesAsignados { get; set; } = new List<Cliente>();
        public List<Progreso> ProgresosRecientes { get; set; } = new List<Progreso>();
        public List<RegistroEntrenamiento> EntrenamientosRecientes { get; set; } = new List<RegistroEntrenamiento>();
    }
}
