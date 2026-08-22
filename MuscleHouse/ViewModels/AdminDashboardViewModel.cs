using System.Collections.Generic;
using MuscleHouse.Models;

namespace MuscleHouse.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalClientes { get; set; }
        public int TotalEntrenadores { get; set; }
        public int TotalRecepcionistas { get; set; }
        public int TotalMembresiasActivas { get; set; }
        public decimal TotalIngresos { get; set; }

        public List<Membresia> MembresiasProximasAVencer { get; set; } = new List<Membresia>();
        public List<Pago> PagosRecientes { get; set; } = new List<Pago>();
        public List<Asistencia> AsistenciasRecientes { get; set; } = new List<Asistencia>();

        // Financial statistics grouping by plan name
        public Dictionary<string, int> VentasPorPlan { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> IngresosPorMetodoPago { get; set; } = new Dictionary<string, decimal>();
    }
}
