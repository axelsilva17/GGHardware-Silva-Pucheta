using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Microsoft.Data.SqlClient;
using GGHardware.Data;
using Microsoft.EntityFrameworkCore;



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
            // Verifica si se ha seleccionado una ruta de archivo
            if (string.IsNullOrEmpty(txtRuta.Text) || txtRuta.Text.Contains("Seleccione una ubicacion"))
            {
                MessageBox.Show("Por favor, selecciona una ubicación para guardar el backup.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Actualiza el estado en la interfaz de usuario
            lblEstado.Text = "🔄 Realizando backup...";
            lblEstado.Foreground = System.Windows.Media.Brushes.Orange;

            string databaseName = "GGHardware";
            string backupPath = txtRuta.Text;

            try
            {
                // Obtener la cadena de conexión del contexto de Entity Framework Core
                string connectionString;
                using (var context = new ApplicationDbContext())
                {
                    // Esta es la línea corregida para EF Core
                    connectionString = context.Database.GetDbConnection().ConnectionString;
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Comando SQL para realizar el backup
                    string backupQuery = $"BACKUP DATABASE [{databaseName}] TO DISK = '{backupPath}' WITH NOFORMAT, NOINIT, NAME = '{databaseName}-Full Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                    using (SqlCommand cmd = new SqlCommand(backupQuery, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // Muestra un mensaje de éxito y actualiza el estado
                MessageBox.Show("¡Backup realizado exitosamente!",
                                "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                lblEstado.Text = "✅ Backup realizado correctamente";
                lblEstado.Foreground = System.Windows.Media.Brushes.Green;
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error al realizar el backup: {ex.Message}\n\nAsegúrate de que el usuario de Windows o el servicio de SQL Server tenga permisos de escritura en la carpeta seleccionada.",
                                "Error de Backup", MessageBoxButton.OK, MessageBoxImage.Error);

                lblEstado.Text = "❌ Error al realizar el backup";
                lblEstado.Foreground = System.Windows.Media.Brushes.Red;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                lblEstado.Text = "❌ Error inesperado";
                lblEstado.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
    }
}
