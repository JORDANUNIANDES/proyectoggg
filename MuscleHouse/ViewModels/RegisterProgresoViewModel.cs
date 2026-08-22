using System.ComponentModel.DataAnnotations;

namespace MuscleHouse.ViewModels
{
    public class RegisterProgresoViewModel
    {
        [Required]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "El peso es obligatorio.")]
        [Range(30, 250, ErrorMessage = "El peso debe estar entre 30 y 250 kg.")]
        public decimal Peso { get; set; }

        [Required(ErrorMessage = "La medida de pecho es obligatoria.")]
        public decimal Pecho { get; set; }

        [Required(ErrorMessage = "La medida de cintura es obligatoria.")]
        public decimal Cintura { get; set; }

        [Required(ErrorMessage = "La medida de brazo es obligatoria.")]
        public decimal Brazo { get; set; }

        [Required(ErrorMessage = "La medida de pierna es obligatoria.")]
        public decimal Pierna { get; set; }

        [Required(ErrorMessage = "La medida de cadera es obligatoria.")]
        public decimal Cadera { get; set; }

        public string Observaciones { get; set; } = string.Empty;
    }
}
