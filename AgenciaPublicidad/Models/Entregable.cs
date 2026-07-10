using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciaPublicidad.Models
{
    [Table("Entregables")]
    public class Entregable
    {
        [Key]
        [Column("entregable_id")]
        public int entregable_id { get; set; }

        [Required(ErrorMessage = "La campaña es obligatoria.")]
        [Column("campana_id")]
        [Display(Name = "Campaña")]
        public int campana_id { get; set; }

        [Required(ErrorMessage = "El diseñador es obligatorio.")]
        [Column("disenador_id")]
        [Display(Name = "Diseñador")]
        public int disenador_id { get; set; }

        [Required(ErrorMessage = "El tipo de entregable es obligatorio.")]
        [Column("tipo")]
        [Display(Name = "Tipo")]
        public string tipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de entrega es obligatoria.")]
        [Column("fecha_entrega")]
        [Display(Name = "Fecha de Entrega")]
        [DataType(DataType.Date)]
        public DateTime fecha_entrega { get; set; } = DateTime.Today;

        // Relaciones
        [ForeignKey("campana_id")]
        public virtual Campana? Campana { get; set; }

        [ForeignKey("disenador_id")]
        public virtual Disenador? Disenador { get; set; }
    }
}
