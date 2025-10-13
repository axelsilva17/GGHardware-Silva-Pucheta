using System;
using System.Linq;
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
            CargarHistorial();
        }

        // Cargar el historial de backups en el DataGrid
        private void CargarHistorial()
        {
            using (var context = new ApplicationDbContext())
            {
                dgBackups.ItemsSource = context.Backups
                                               .OrderByDescending(b => b.Fecha)
                                               .ToList();
            }
        }

        // Seleccionar ruta para guardar backup
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

        // Realizar backup y guardar registro
        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtRuta.Text))
            {
                MessageBox.Show("Seleccione una ubicación para guardar el backup.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            lblEstado.Text = "🔄 Realizando backup...";
            lblEstado.Foreground = System.Windows.Media.Brushes.Orange;

            string databaseName = "GGHardware";
            string backupPath = txtRuta.Text;

            try
            {
                string connectionString;
                using (var context = new ApplicationDbContext())
                {
                    connectionString = context.Database.GetDbConnection().ConnectionString;
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string backupQuery = $"BACKUP DATABASE [{databaseName}] TO DISK = '{backupPath}' " +
                                         "WITH NOFORMAT, NOINIT, NAME = '{databaseName}-Full Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                    using (SqlCommand cmd = new SqlCommand(backupQuery, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // Guardar registro del backup en la DB
                using (var context = new ApplicationDbContext())
                {
                    context.Backups.Add(new Models.BackupRegistro
                    {
                        NombreArchivo = System.IO.Path.GetFileName(backupPath),
                        Fecha = DateTime.Now,
                        RutaArchivo = backupPath
                    });
                    context.SaveChanges();
                }

                lblEstado.Text = "✅ Backup realizado correctamente";
                lblEstado.Foreground = System.Windows.Media.Brushes.Green;

                CargarHistorial();
            }
            catch (Exception ex)
            {
                lblEstado.Text = "❌ Error al realizar el backup";
                lblEstado.Foreground = System.Windows.Media.Brushes.Red;
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Restaurar backup seleccionado
        private void RestaurarBackup_Click(object sender, RoutedEventArgs e)
        {
            var backup = dgBackups.SelectedItem as Models.BackupRegistro;
            if (backup == null) return;

            if (MessageBox.Show($"¿Desea restaurar el backup '{backup.NombreArchivo}'? Esto reemplazará la base de datos actual.",
                                "Confirmar restauración", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            string databaseName = "GGHardware";
            string ruta = backup.RutaArchivo;

            try
            {
                string connectionString;
                using (var context = new ApplicationDbContext())
                {
                    connectionString = context.Database.GetDbConnection().ConnectionString;
                }

                // NUEVO CÓDIGO: Conectarse a master
                var builder = new SqlConnectionStringBuilder(connectionString)
                {
                    InitialCatalog = "master"
                };

                using (SqlConnection con = new SqlConnection(builder.ConnectionString))
                {
                    con.Open();
                    string sql = $@"
                ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                RESTORE DATABASE [{databaseName}] FROM DISK = N'{ruta}' WITH REPLACE;
                ALTER DATABASE [{databaseName}] SET MULTI_USER;
            ";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Base de datos restaurada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarHistorial();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al restaurar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
