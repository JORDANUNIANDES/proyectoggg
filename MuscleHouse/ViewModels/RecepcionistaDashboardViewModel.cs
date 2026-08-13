using System.Collections.Generic;
using MuscleHouse.Models;

namespace MuscleHouse.ViewModels
{
    public class RecepcionistaDashboardViewModel
    {
        public int TotalClientesActivos { get; set; }
        public decimal IngresosTotales { get; set; }
        public List<Cliente> ClientesRecientes { get; set; } = new List<Cliente>();
        public List<Membresia> MembresiasProximasAVencer { get; set; } = new List<Membresia>();
        public List<Pago> PagosRecientes { get; set; } = new List<Pago>();
        public List<Asistencia> AsistenciasDeHoy { get; set; } = new List<Asistencia>();
    }
}
