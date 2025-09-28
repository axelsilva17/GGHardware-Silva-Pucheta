using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GGHardware.Models
{
    [Table("Venta")] // Usando el nombre de tu tabla
    public class Venta : INotifyPropertyChanged
    {
        [Key]
        public int id_venta { get; set; } // Cambié de id_Venta a id_venta para consistencia

        public DateTime Fecha { get; set; }

        public double Monto { get; set; }

        // Campos nuevos para comprobantes
        [MaxLength(20)]
        public string NumeroComprobante { get; set; }

        public int? IdTipoComprobante { get; set; }

        public int PuntoVenta { get; set; } = 1;

        public int? NumeroSecuencial { get; set; }

        [MaxLength(50)]
        public string MetodoPago { get; set; }

        public double? MontoRecibido { get; set; }

        // Campo calculado para vuelto
        [NotMapped]
        public double Vuelto => (MontoRecibido ?? 0) - Monto;

        [MaxLength(500)]
        public string Observaciones { get; set; }

        [MaxLength(20)]
        public string Estado { get; set; } = "Completada";

        // Claves foráneas
        public int id_Cliente { get; set; }
        public int id_Usuario { get; set; }

        // Propiedades de navegación
        [ForeignKey("id_Cliente")]
        public virtual Cliente Cliente { get; set; }

        [ForeignKey("id_Usuario")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("IdTipoComprobante")]
        public virtual TipoComprobante ?TipoComprobante { get; set; }

        // Relación con detalle
        public virtual ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();

        // Para binding en la UI
        [NotMapped]
        public string ClienteNombre => Cliente?.NombreCompleto ?? "Cliente no especificado";

        [NotMapped]
        public string TipoComprobanteNombre => TipoComprobante?.Nombre?? "Sin especificar";

        [NotMapped]
        public string NumeroComprobanteFormateado => string.IsNullOrEmpty(NumeroComprobante) ? "Sin número" : NumeroComprobante;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}