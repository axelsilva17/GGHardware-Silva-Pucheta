using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace GGHardware.Models
{
    [Table("Producto")]
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

        [Column(TypeName = "decimal(18,2)")]
        public decimal precio_costo { get; set; }

        private decimal _precio_venta;

        [Column(TypeName = "decimal(18,2)")]
        public decimal precio_venta
        {
            get => _precio_venta;
            set { _precio_venta = value; OnPropertyChanged(); }

        }

        [MaxLength(255)]
        public string descripcion { get; set; }

        public int stock_min { get; set; }

        private int _stock;
        public int Stock
        {
            get => _stock;
            set
            {
                _stock = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieneStock));
            }
        }


        public int id_categoria { get; set; }   
        [ForeignKey("id_categoria")]
        public Categoria Categoria { get; set; }

        public string NombreCategoria
        {
            get { return Categoria?.nombre ?? "Sin categoría"; }
        }

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

        public bool Activo { get; set; } = true;

        [NotMapped]
        public string EstadoStock => Stock <= 0 ? "Sin Stock" :
                                    Stock <= stock_min ? "Stock Bajo" : "Disponible";

        public event PropertyChangedEventHandler PropertyChanged;

        public int? id_proveedor { get; set; }

        [ForeignKey("id_proveedor")]
        public virtual Proveedor? Proveedor { get; set; }

        [NotMapped]
        public string NombreProveedor => Proveedor?.razon_social ?? "Sin proveedor";

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}