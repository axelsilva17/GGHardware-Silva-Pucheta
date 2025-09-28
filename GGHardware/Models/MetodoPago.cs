using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GGHardware.Models
{
    [Table("MetodosPago")]
    public class MetodoPago
    {
        [Key]
        public int id_metodo { get; set; }

        [Required]
        [MaxLength(50)]
        public string nombre { get; set; } // "Efectivo", "Tarjeta de Débito", etc.

        public bool requiere_vuelto { get; set; } = false;

        public bool activo { get; set; } = true;

        // Propiedades para la UI
        [NotMapped]
        public string DescripcionCompleta => requiere_vuelto ?
            $"{nombre} (Requiere vuelto)" : nombre;
    }
}