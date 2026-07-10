using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciaPublicidad.Models
{
    [Table("Campanas")]
    public class Campana
    {
        [Key]
        [Column("campana_id")]
        public int campana_id { get; set; }

        [Required(ErrorMessage = "El cliente es obligatorio.")]
        [Column("cliente_id")]
        [Display(Name = "Cliente")]
        public int cliente_id { get; set; }

        [Required(ErrorMessage = "El nombre de la campaña es obligatorio.")]
        [Column("nombre")]
        [Display(Name = "Nombre")]
        public string nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El presupuesto es obligatorio.")]
        [Column("presupuesto", TypeName = "decimal(18,2)")]
        [Display(Name = "Presupuesto")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El presupuesto debe ser mayor a 0.")]
        public decimal presupuesto { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [Column("fecha_inicio")]
        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        public DateTime fecha_inicio { get; set; } = DateTime.Today;

        // Relaciones
        [ForeignKey("cliente_id")]
        public virtual Cliente? Cliente { get; set; }

        public virtual ICollection<Entregable> Entregables { get; set; } = new List<Entregable>();
    }
}
