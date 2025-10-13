using GGHardware.Models;
using GGHardware.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static GGHardware.Services.SolicitudRestauracionService;
namespace GGHardware.ViewModels
{
    public class SolicitudRestauracionViewModel : INotifyPropertyChanged
    {
        private readonly SolicitudRestauracionService _service;
        private int _id_usuario_actual;

        private ObservableCollection<BackupDTO> _backupsDisponibles;
        public ObservableCollection<BackupDTO> BackupsDisponibles
        {
            get { return _backupsDisponibles; }
            set
            {
                _backupsDisponibles = value;
                OnPropertyChanged(nameof(BackupsDisponibles));
            }
        }

        private BackupDTO _backupSeleccionado;
        public BackupDTO BackupSeleccionado
        {
            get { return _backupSeleccionado; }
            set
            {
                _backupSeleccionado = value;
                if (value != null)
                {
                    FechaBackup = value.fecha.ToString("dd/MM/yyyy");
                }
                else
                {
                    FechaBackup = string.Empty;
                }
                OnPropertyChanged(nameof(BackupSeleccionado));
            }
        }

        private string _fechaBackup;
        public string FechaBackup
        {
            get { return _fechaBackup; }
            set
            {
                _fechaBackup = value;
                OnPropertyChanged(nameof(FechaBackup));
            }
        }

        private DateTime _fechaRestauracion = DateTime.Now;
        public DateTime FechaRestauracion
        {
            get { return _fechaRestauracion; }
            set
            {
                _fechaRestauracion = value;
                OnPropertyChanged(nameof(FechaRestauracion));
            }
        }

        private string _motivo;
        public string Motivo
        {
            get { return _motivo; }
            set
            {
                _motivo = value;
                OnPropertyChanged(nameof(Motivo));
            }
        }

        private ObservableCollection<SolicitudRestauracionVM> _misSolicitudes;
        public ObservableCollection<SolicitudRestauracionVM> MisSolicitudes
        {
            get { return _misSolicitudes; }
            set
            {
                _misSolicitudes = value;
                OnPropertyChanged(nameof(MisSolicitudes));
            }
        }

        private string _mensaje;
        public string Mensaje
        {
            get { return _mensaje; }
            set
            {
                _mensaje = value;
                OnPropertyChanged(nameof(Mensaje));
            }
        }

        public RelayCommand EnviarSolicitudCommand { get; }
        public RelayCommand CargarBackupsCommand { get; }
        public RelayCommand CargarMisSolicitudesCommand { get; }

        public SolicitudRestauracionViewModel(SolicitudRestauracionService service, int idUsuario)
        {
            _service = service;
            _id_usuario_actual = idUsuario;

            BackupsDisponibles = new ObservableCollection<BackupDTO>();
            MisSolicitudes = new ObservableCollection<SolicitudRestauracionVM>();

            EnviarSolicitudCommand = new RelayCommand(async _ => await EnviarSolicitud());
            CargarBackupsCommand = new RelayCommand(async _ => await CargarBackups());
            CargarMisSolicitudesCommand = new RelayCommand(async _ => await CargarMisSolicitudes());

            // Cargar datos al inicializar
            CargarBackupsCommand.Execute(null);
            CargarMisSolicitudesCommand.Execute(null);
        }

        private async System.Threading.Tasks.Task CargarBackups()
        {
            try
            {
                var backups = await _service.ObtenerBackupsDisponibles();

                BackupsDisponibles.Clear();
                foreach (var backup in backups)
                {
                    BackupsDisponibles.Add(backup);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar backups: {ex.Message}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task CargarMisSolicitudes()
        {
            try
            {
                var solicitudes = await _service.ObtenerSolicitudesPorSupervisor(_id_usuario_actual);

                MisSolicitudes.Clear();
                foreach (var solicitud in solicitudes)
                {
                    MisSolicitudes.Add(solicitud);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar solicitudes: {ex.Message}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task EnviarSolicitud()
        {
            try
            {
                // Validaciones
                if (BackupSeleccionado == null)
                {
                    MessageBox.Show("Debe seleccionar un backup",
                                  "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Motivo))
                {
                    MessageBox.Show("Debe ingresar un motivo",
                                  "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Crear solicitud
                int id_solicitud = await _service.CrearSolicitud(
                    _id_usuario_actual,
                    BackupSeleccionado.id,
                    Motivo
                );

                MessageBox.Show($"Solicitud enviada exitosamente.\nID de solicitud: {id_solicitud}",
                              "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiar formulario
                BackupSeleccionado = null;
                Motivo = string.Empty;
                FechaRestauracion = DateTime.Now;
                Mensaje = string.Empty;

                // Recargar solicitudes
                await CargarMisSolicitudes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar solicitud: {ex.Message}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // RelayCommand simple para MVVM
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => _execute(parameter);
    }
}