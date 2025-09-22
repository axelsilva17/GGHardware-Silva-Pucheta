using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GGHardware.Models
{
    [Table("Clientes")]
    public class Cliente
    {
        [Key]
        public int id_cliente { get; set; }
        public bool Activo { get; set; } = true;

        [MaxLength(11)]
        public required string cuit { get; set; }

        [MaxLength(50)]
        public required string nombre { get; set; }

        [MaxLength(50)]
        public required string apellido { get; set; }

        [MaxLength(25)]
        public required string telefono { get; set; }

        [MaxLength(100)]
        public required string direccion { get; set; }

        [MaxLength(100)]
        public required string provincia { get; set; }

        [MaxLength(100)]
        public required string localidad { get; set; }

        [MaxLength(100)]
        public required string condicion_fiscal { get; set; }
        public string EstadoTexto => Activo ? "Activo" : "Inactivo";

    }
}