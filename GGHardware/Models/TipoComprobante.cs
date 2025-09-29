using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GGHardware.Models
{
    [Table("TipoComprobante")]
    public class TipoComprobante
    {
        [Key]
        public int id_tipo { get; set; }

        [Required]
        [MaxLength(5)]
        public string ?codigo { get; set; }

        [Required]
        [MaxLength(50)]
        public string ?Nombre { get; set; }

        public bool activo { get; set; } = true;

        public bool requiere_cuit { get; set; } = false;
    }
}
