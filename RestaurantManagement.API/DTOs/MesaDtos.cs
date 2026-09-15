using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.API.DTOs
{
    public class MesaCreateDto
    {
        [Range(1, 1000, ErrorMessage = "El número de mesa debe ser un entero positivo.")]
        public int Numero { get; set; }

        [Range(1, 100, ErrorMessage = "La capacidad debe ser mayor a 0.")]
        public int Capacidad { get; set; }

        public string Ubicacion { get; set; } = string.Empty;

        public string Estado { get; set; } = "Disponible"; // Disponible, Ocupada, Reservada, Mantenimiento
    }

    public class MesaUpdateDto : MesaCreateDto
    {
    }

    public class MesaDto
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public int Capacidad { get; set; }
        public string Ubicacion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
