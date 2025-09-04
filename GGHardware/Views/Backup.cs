using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;



namespace GGHardware.Views
{
    public partial class Backup : UserControl
    {
        public Backup()
        {
            InitializeComponent();
        }

        private void btnSeleccionarRuta_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Title = "Guardar Backup",
                Filter = "Archivo de respaldo (*.bak)|*.bak",
                FileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmm}.bak"
            };

            if (dlg.ShowDialog() == true)
            {
                txtRuta.Text = dlg.FileName;
            }
        }
        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            // Simulación de acción de backup (sin DB)
            lblEstado.Text = "🔄 Simulando backup...";

            // Podés mostrar otra vista, o solo un mensaje
            MessageBox.Show("Aquí se mostraría la vista de Backup.",
                            "Vista Backup", MessageBoxButton.OK, MessageBoxImage.Information);

            // Estado final
            lblEstado.Text = "✅ Vista de Backup cargada correctamente";
            lblEstado.Foreground = System.Windows.Media.Brushes.Green;
        }

    }
}
