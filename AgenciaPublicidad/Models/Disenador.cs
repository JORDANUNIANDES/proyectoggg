using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciaPublicidad.Models
{
    [Table("Disenadores")]
    public class Disenador
    {
        [Key]
        [Column("disenador_id")]
        public int disenador_id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [Column("nombre")]
        [Display(Name = "Nombre")]
        public string nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La especialidad es obligatoria.")]
        [Column("especialidad")]
        [Display(Name = "Especialidad")]
        public string especialidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        [Column("email")]
        [Display(Name = "Email")]
        public string email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [Column("telefono")]
        [Display(Name = "Teléfono")]
        public string telefono { get; set; } = string.Empty;

        // Relaciones
        public virtual ICollection<Entregable> Entregables { get; set; } = new List<Entregable>();
    }
}
