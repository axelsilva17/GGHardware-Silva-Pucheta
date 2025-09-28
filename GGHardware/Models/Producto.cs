using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GGHardware.Models
{
    [Table("Productos")]
    public class Producto : INotifyPropertyChanged
    {
        [Key]
        public int Id_Producto { get; set; }

        private string _nombre;
        [MaxLength(100)]
        public string? Nombre
        {
            get => _nombre;
            set
            {
                _nombre = value;
                OnPropertyChanged(nameof(Nombre));
            }
        }

        public double precio_costo { get; set; }

        private double _precio_venta;
        public double precio_venta
        {
            get => _precio_venta;
            set
            {
                _precio_venta = value;
                OnPropertyChanged(nameof(precio_venta));
            }
        }

        [MaxLength(255)]
        public string? descripcion { get; set; }

        public double stock_min { get; set; }

        private double _stock;
        public double Stock
        {
            get => _stock;
            set
            {
                _stock = value;
                OnPropertyChanged(nameof(Stock));
            }
        }

        [MaxLength(50)]
        public string? categoria { get; set; }

        public DateTime fecha_creacion { get; set; } = DateTime.Now;

        public bool activo { get; set; } = true;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}