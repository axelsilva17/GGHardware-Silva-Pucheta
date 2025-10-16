﻿using System;
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

        private void SolicitudRestauracionView_Loaded(object sender, RoutedEventArgs e)
        {
            CargarSolicitudes();
        }

        private void CargarSolicitudes()
        {
            var viewModel = DataContext as SolicitudRestauracionViewModel;
            if (viewModel != null)
            {
                // Ejecutar el comando, NO llamar al método directamente
                viewModel.CargarMisSolicitudesCommand?.Execute(null);
            }
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

        public void RefrescarSolicitudes()
        {
            CargarSolicitudes();
        }
    }
}