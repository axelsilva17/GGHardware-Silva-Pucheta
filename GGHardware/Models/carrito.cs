using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GGHardware.Models
{
    public class CarritoItem : INotifyPropertyChanged
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }

        private int _cantidad;
        public int Cantidad
        {
            get => _cantidad;
            set
            {
                _cantidad = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        //Para manejar descuentos individuales
        private decimal? _precioConDescuento;
        public decimal? PrecioConDescuento
        {
            get => _precioConDescuento;
            set
            {
                _precioConDescuento = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
                OnPropertyChanged(nameof(TieneDescuento));
                OnPropertyChanged(nameof(PorcentajeDescuento));
            }
        }

        // Propiedades calculadas
        public decimal Subtotal => (PrecioConDescuento ?? Precio) * Cantidad;

        [NotMapped]
        public bool TieneDescuento => PrecioConDescuento.HasValue && PrecioConDescuento < Precio;

        [NotMapped]
        public double PorcentajeDescuento => TieneDescuento ?
            (double)(((Precio - PrecioConDescuento.Value) / Precio) * 100) : 0;

        [NotMapped]
        public decimal MontoDescuento => TieneDescuento ?
            (Precio - PrecioConDescuento.Value) * Cantidad : 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // aplicar descuento por porcentaje
        public void AplicarDescuentoPorcentaje(double porcentaje)
        {
            if (porcentaje < 0 || porcentaje > 100) return;
            var descuento = (Precio * (decimal)porcentaje) / 100;
            PrecioConDescuento = Precio - descuento;
        }

        // quitar descuento
        public void QuitarDescuento()
        {
            PrecioConDescuento = null;
        }
    }
}