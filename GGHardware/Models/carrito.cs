using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGHardware.Models
{
    public class CarritoItem
    {
        public int IdProducto { get; set; }
        public string? Nombre { get; set; }
        public double  Precio { get; set; }
        public double Cantidad { get; set; }
    }

    public class Producto
    {

        [Key]
        public int Id_Producto { get; set; }
        public string? Nombre { get; set; }
        public double precio_costo { get; set; }

        public double precio_venta { get; set; }
        public char descripcion{ get; set; }
        public double stock_min { get; set; }
        public double Stock { get; set; }
    }
}