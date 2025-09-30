using System;

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
}