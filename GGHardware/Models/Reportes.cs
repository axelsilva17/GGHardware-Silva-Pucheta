using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GGHardware.Models
{
    [Table("Reportes", Schema = "dbo")]
    public class Reporte
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReporteID { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoReporte { get; set; } // 'VentasDiarias', 'VentasPeriodo', 'ProductosVendidos', etc.

        [Required]
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "date")]
        public DateTime FechaInicio { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime FechaFin { get; set; }

        [Required]
        public int UsuarioID { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalVentas { get; set; }

        [Required]
        public int TotalProductos { get; set; }

        [Required]
        public int TotalRegistros { get; set; }

        [StringLength(500)]
        public string Observaciones { get; set; }

        [Required]
        public bool Activo { get; set; } = true;

        // Navegación
        [ForeignKey("UsuarioID")]
        public virtual Usuario Usuario { get; set; }
    }

    namespace GGHardware.Models
    {
        public class ReporteVentaDetalle
        {
            public DateTime Fecha { get; set; }
            public string Cliente { get; set; }
            public string Producto { get; set; }
            public int Cantidad { get; set; }
            public decimal Total { get; set; }
        }

        public class ResumenVentas
        {
            public decimal VentasTotales { get; set; }
            public int ProductosVendidos { get; set; }
            public int ClientesNuevos { get; set; }
            public int TotalRegistros { get; set; }
        }
    }
}