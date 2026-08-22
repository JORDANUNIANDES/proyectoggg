using System.ComponentModel.DataAnnotations;

namespace MuscleHouse.ViewModels
{
    public class ChatQueryViewModel
    {
        [Required(ErrorMessage = "El mensaje no puede estar vacío.")]
        public string Message { get; set; } = string.Empty;
    }
}
