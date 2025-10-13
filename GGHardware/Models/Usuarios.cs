using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace GGHardware.Models
{
    [Table("Usuarios")] // <-- Nombre de la tabla en SQL (ajústalo si es distinto)
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_usuario { get; set; }

        public bool Activo { get; set; } = true;

        [Required]
        [MaxLength(11)] // ajusta según el tamaño de tu BD
        public int dni { get; set; } 

        [Required]
        [MaxLength(50)]
        public required string Nombre { get; set; }


        [Required]
        [MaxLength(50)]
        public required string apellido { get; set; } 

        [Required]
        [MaxLength(100)]
        public required string correo { get; set; } 

        [Required]
        [MaxLength(100)]
        public required string contraseña { get; set; }

        [Required]
        public required int RolId { get; set; }

        // Solicitudes donde es supervisor
        [InverseProperty("Supervisor")]
        public virtual ICollection<SolicitudRestauracion> SolicitudesComoSupervisor { get; set; }

        // Solicitudes donde es gerente
        [InverseProperty("Gerente")]
        public virtual ICollection<SolicitudRestauracion> SolicitudesComoGerente { get; set; }

        public Usuario()
        {
            SolicitudesComoSupervisor = new List<SolicitudRestauracion>();
            SolicitudesComoGerente = new List<SolicitudRestauracion>();
        }

        [Column(TypeName = "date")]
       public DateTime? fecha_Nacimiento { get; set; } 
        
       [NotMapped]  // No se guarda en la base de datos
        public string NombreRol { get; set; } = string.Empty;

        [NotMapped] // para que EF no intente guardarla en la DB
        public string EstadoTexto { get; set; }


    }
}
