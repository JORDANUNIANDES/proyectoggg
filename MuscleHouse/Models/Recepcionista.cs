namespace MuscleHouse.Models
{
    public class Recepcionista
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string HorarioAtencion { get; set; } = string.Empty;
        public string Fotografia { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
