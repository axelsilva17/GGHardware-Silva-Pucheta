using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GGHardware.Models
{
    [Table("Proveedores")]
    public class Proveedor
    {
        [Key]
        public int id_proveedor { get; set; }

        [Required(ErrorMessage = "La razón social es obligatoria")]
        [StringLength(200)]
        public string razon_social { get; set; }

        [StringLength(100)]
        public string? nombre_contacto { get; set; }

        [StringLength(13)]
        public string? cuit { get; set; }

        [StringLength(20)]
        public string? telefono { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string? email { get; set; }

        [StringLength(250)]
        public string? direccion { get; set; }

        [StringLength(10)]
        public string? codigo_postal { get; set; }

        public bool activo { get; set; } = true;

        public DateTime fecha_alta { get; set; } = DateTime.Now;

        // Propiedad para mostrar en UI
        [NotMapped]
        public string DatosCompletos =>
            $"{razon_social}{(string.IsNullOrEmpty(cuit) ? "" : $" - CUIT: {cuit}")}";
    }
}