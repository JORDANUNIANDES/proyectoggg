using System.ComponentModel.DataAnnotations;

namespace MuscleHouse.ViewModels
{
    public class RegisterWorkoutLogViewModel
    {
        [Required]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "El ejercicio es obligatorio.")]
        public int EjercicioId { get; set; }

        [Required(ErrorMessage = "Las series son obligatorias.")]
        [Range(1, 10, ErrorMessage = "Las series deben estar entre 1 y 10.")]
        public int Series { get; set; }

        [Required(ErrorMessage = "Las repeticiones son obligatorias.")]
        [Range(1, 100, ErrorMessage = "Las repeticiones deben estar entre 1 y 100.")]
        public int Repeticiones { get; set; }

        [Required(ErrorMessage = "El peso es obligatorio.")]
        public decimal Peso { get; set; }

        [Range(1, 10, ErrorMessage = "El esfuerzo RPE debe estar entre 1 y 10.")]
        public int? RPE { get; set; }

        public string Observaciones { get; set; } = string.Empty;
    }
}
