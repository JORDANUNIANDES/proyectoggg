using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciaPublicidad.Models
{
    [Table("Clientes")]
    public class Cliente
    {
        [Key]
        [Column("cliente_id")]
        public int cliente_id { get; set; }

        [Required(ErrorMessage = "El nombre de la empresa es obligatorio.")]
        [Column("nombre_empresa")]
        [Display(Name = "Nombre de Empresa")]
        public string nombre_empresa { get; set; } = string.Empty;

        [Required(ErrorMessage = "El contacto es obligatorio.")]
        [Column("contacto")]
        [Display(Name = "Contacto")]
        public string contacto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [Column("telefono")]
        [Display(Name = "Teléfono")]
        public string telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        [Column("email")]
        [Display(Name = "Email")]
        public string email { get; set; } = string.Empty;

        // Relaciones
        public virtual ICollection<Campana> Campanas { get; set; } = new List<Campana>();
    }
}
