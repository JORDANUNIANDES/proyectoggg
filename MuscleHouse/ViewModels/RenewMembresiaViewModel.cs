using System.ComponentModel.DataAnnotations;

namespace MuscleHouse.ViewModels
{
    public class RenewMembresiaViewModel
    {
        [Required]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un plan.")]
        public int PlanId { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        public string MetodoPago { get; set; } = "Efectivo";
    }
}
