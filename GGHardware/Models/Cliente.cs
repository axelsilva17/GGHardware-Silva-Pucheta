using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GGHardware.Models
{
    [Table("Clientes")]
    public class Cliente : INotifyPropertyChanged
    {
        [Key]
        public int id_cliente { get; set; }
        public bool Activo { get; set; } = true;

        [MaxLength(11)]
        public required string cuit { get; set; }

        private string _nombre;
        [MaxLength(50)]
        public required string nombre
        {
            get => _nombre;
            set
            {
                _nombre = value;
                OnPropertyChanged(nameof(nombre));
                OnPropertyChanged(nameof(NombreCompleto));
            }
        }

        private string _apellido;
        [MaxLength(50)]
        public required string apellido
        {
            get => _apellido;
            set
            {
                _apellido = value;
                OnPropertyChanged(nameof(apellido));
                OnPropertyChanged(nameof(NombreCompleto));
            }
        }

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

        // Propiedad calculada para mostrar en la UI
        public string NombreCompleto => $"{nombre} {apellido} - CUIT: {cuit}";

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}