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
        [MaxLength(20)]
        public required string rol { get; set; }

        [Column(TypeName = "date")]
        public DateTime? fecha_Nacimiento { get; set; } 
    }
}
