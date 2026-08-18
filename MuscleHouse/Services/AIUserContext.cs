using System;
using System.Collections.Generic;

namespace MuscleHouse.Services
{
    public class AIUserContext
    {
        public int ClienteId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Objetivo { get; set; } = string.Empty;

        // Physical Measurements
        public decimal? PesoActual { get; set; }
        public decimal? Pecho { get; set; }
        public decimal? Cintura { get; set; }
        public decimal? Brazo { get; set; }
        public decimal? Pierna { get; set; }
        public decimal? Cadera { get; set; }
        public string? UltimasObservacionesProgreso { get; set; }
        public DateTime? FechaUltimoProgreso { get; set; }

        // Active Routine
        public string? NombreRutinaActiva { get; set; }
        public List<AIRoutineExercise> EjerciciosRutina { get; set; } = new();

        // Workout Logs & Performance
        public List<AIWorkoutLog> UltimosRegistrosEntrenamiento { get; set; } = new();
        public decimal? IncrementoSobrecargaProgresiva { get; set; }
    }

    public class AIRoutineExercise
    {
        public string NombreEjercicio { get; set; } = string.Empty;
        public string GrupoMuscular { get; set; } = string.Empty;
        public int Series { get; set; }
        public int Repeticiones { get; set; }
        public decimal PesoRecomendado { get; set; }
        public int DescansoSegundos { get; set; }
    }

    public class AIWorkoutLog
    {
        public string NombreEjercicio { get; set; } = string.Empty;
        public int Series { get; set; }
        public int Repeticiones { get; set; }
        public decimal Peso { get; set; }
        public int? RPE { get; set; }
        public DateTime Fecha { get; set; }
    }
}
