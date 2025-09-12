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
        public int Precio { get; set; }
        public int Cantidad { get; set; }
    }

    public class Producto
    {

        [Key]
        public int Id_Producto { get; set; }
        public string? Nombre { get; set; }
        public float precio_costo { get; set; }

        public int precio_venta { get; set; }
        public char descripcion{ get; set; }
        public float stock_min { get; set; }
        public float Stock { get; set; }
    }
}