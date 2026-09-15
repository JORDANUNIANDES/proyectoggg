using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.API.DTOs
{
    public class PlatoCreateDto
    {
        [Required(ErrorMessage = "El nombre del plato es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Range(0.01, 10000.00, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public string Categoria { get; set; } = string.Empty;

        public bool Disponible { get; set; } = true;
    }

    public class PlatoUpdateDto : PlatoCreateDto
    {
    }

    public class PlatoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public bool Disponible { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
