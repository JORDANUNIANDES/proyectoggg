using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuscleHouse.ViewModels
{
    public class CreateRutinaViewModel
    {
        [Required]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "El nombre de la rutina es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        public List<RutinaEjercicioItemViewModel> Ejercicios { get; set; } = new List<RutinaEjercicioItemViewModel>();
    }

    public class RutinaEjercicioItemViewModel
    {
        public int EjercicioId { get; set; }
        public int Series { get; set; }
        public int Repeticiones { get; set; }
        public decimal PesoRecomendado { get; set; }
        public int DescansoSegundos { get; set; }
        public int Orden { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }
}
