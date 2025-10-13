using GGHardware.Models;
using GGHardware.Views;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GGHardware.Models
{
    [Table("SolicitudRestauracion", Schema = "dbo")]
    public class SolicitudRestauracion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_solicitud { get; set; }

        [Required]
        [ForeignKey("Supervisor")]
        public int id_supervisor { get; set; }

        [ForeignKey("Gerente")]
        public int? id_gerente { get; set; }

        [Required]
        [ForeignKey("Backup")]
        public int id_backup { get; set; }

        [Required]
        public DateTime fecha_solicitud { get; set; }

        [Required]
        public DateTime fecha_backup { get; set; }

        [Required]
        [StringLength(50)]
        public string estado { get; set; } // Pendiente, Aprobada, Rechazada, Completada, EnRestauracion

        [StringLength(500)]
        public string motivo_solicitud { get; set; }

        [StringLength(500)]
        public string observaciones_gerente { get; set; }

        public DateTime? fecha_aprobacion { get; set; }

        public DateTime? fecha_restauracion { get; set; }

        // Propiedades de navegación
        [InverseProperty("SolicitudesComoSupervisor")]
        public virtual Usuario Supervisor { get; set; }

        [InverseProperty("SolicitudesComoGerente")]
        public virtual Usuario Gerente { get; set; }

        public virtual Backup Backup { get; set; }
    }
}