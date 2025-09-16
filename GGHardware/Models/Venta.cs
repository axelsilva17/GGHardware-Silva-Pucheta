using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        [ForeignKey("id_Cliente")]
        public virtual Cliente Cliente { get; set; }

        [ForeignKey("id_Usuario")]
        public virtual Usuario Usuario { get; set; }

    }
}
