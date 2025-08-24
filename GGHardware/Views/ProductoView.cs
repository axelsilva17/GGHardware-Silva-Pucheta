using System.Windows;
using System.Windows.Controls;

namespace GGHardware.Views
{
    public partial class ProductoView : UserControl
    {
        public ProductoView()
        {
            InitializeComponent();

            // Para verificar que se carga correctamente
            MessageBox.Show("ProductoView cargado");
        }
    }
}
