using System;
using System.Windows;
using System.Windows.Controls;
using GGHardware.ViewModels;

namespace GGHardware.Views
{
    public partial class SolicitudRestauracionView : UserControl
    {
        public SolicitudRestauracionView()
        {
            InitializeComponent();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as SolicitudRestauracionViewModel;
            if (viewModel != null)
            {
                viewModel.BackupSeleccionado = null;
                viewModel.Motivo = string.Empty;
                viewModel.FechaRestauracion = DateTime.Now;
                viewModel.Mensaje = string.Empty;
            }
        }
    }
}