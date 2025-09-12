using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGHardware.Models
{
    public class Venta
    {


        [Key]
        public int id_Venta { get; set; }
        public DateTime Fecha { get; set; }
        public double Monto { get; set; }

        // 🔗 Claves foráneas
        public int id_Cliente { get; set; }
        public int id_Usuario { get; set; }

        // 🚀 Propiedades de navegación (opcionales, pero recomendadas)
        public Cliente? Clientes { get; set; }
        public Usuario? Usuarios { get; set; }

    }
}
