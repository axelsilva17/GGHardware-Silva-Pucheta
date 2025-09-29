using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace GGHardware.Models
{
    [Table("Productos")]
    public class Producto : INotifyPropertyChanged
    {
        [Key]
        public int Id_Producto { get; set; }

        private string nombre;
        [MaxLength(100)]
        public string Nombre
        {
            get => nombre;
            set
            {
                nombre = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        [MaxLength(255)]
        public string descripcion { get; set; }

        public double stock_min { get; set; }

        private double _stock;
        public double Stock
        {
            get => _stock;
            set
            {
                _stock = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieneStock));
            }
        }

        [MaxLength(50)]
        public string categoria { get; set; }

        public DateTime fecha_creacion { get; set; } = DateTime.Now;

        public bool activo { get; set; } = true;

        // NUEVOS CAMPOS para búsqueda por código
        [MaxLength(50)]
        public string codigo_barras { get; set; }

        [MaxLength(20)]
        public string codigo_interno { get; set; }

        // Propiedades calculadas para la UI
        [NotMapped]
        public bool TieneStock => Stock > 0;

        [NotMapped]
        public bool StockBajo => Stock <= stock_min;

        [NotMapped]
        public string EstadoStock => Stock <= 0 ? "Sin Stock" :
                                    Stock <= stock_min ? "Stock Bajo" : "Disponible";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}