using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GGHardware.Models
{
    [Table("DetalleVenta")]
    public class DetalleVenta
    {
        [Key]
        public int id_detalle { get; set; }

        public int id_venta { get; set; }

        public int id_producto { get; set; }

        [Required]
        [MaxLength(200)]
        public string nombre_producto { get; set; } // Guardamos el nombre para mantener histórico

        public int cantidad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal precio_unitario { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? precio_con_descuento { get; set; }

        // Propiedades de navegación
        [ForeignKey("id_venta")]
        public virtual Venta Venta { get; set; }

        [ForeignKey("id_producto")]
        public virtual Producto Producto { get; set; }

        // Propiedades calculadas
        [NotMapped]
        public decimal PrecioFinal => precio_con_descuento ?? precio_unitario;

        [NotMapped]
        public decimal Subtotal => PrecioFinal * cantidad;

        [NotMapped]
        public bool TieneDescuento => precio_con_descuento.HasValue && precio_con_descuento < precio_unitario;

        [NotMapped]
        public decimal MontoDescuento => TieneDescuento ?
            (precio_unitario - precio_con_descuento.Value) * cantidad : 0;

        [NotMapped]
        public double PorcentajeDescuento => TieneDescuento ?
            (double)(((precio_unitario - precio_con_descuento.Value) / precio_unitario) * 100) : 0;
    }
}