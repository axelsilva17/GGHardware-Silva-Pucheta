using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGHardware.Models
{
    [Table("Backups")]
    public class BackupRegistro
    {
        public int Id { get; set; }
        public string NombreArchivo { get; set; }
        public DateTime Fecha { get; set; }
        public string RutaArchivo { get; set; }
    }

}
