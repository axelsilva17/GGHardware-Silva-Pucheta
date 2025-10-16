using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GGHardware.Data;
using GGHardware.Models;
using Microsoft.EntityFrameworkCore;

namespace GGHardware.Services
{
    public class SolicitudRestauracionService
    {
        private readonly ApplicationDbContext _context;

        public SolicitudRestauracionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CrearSolicitud(int id_supervisor, int id_backup, string motivo)
        {
            var backup = await _context.Backups
                .FirstOrDefaultAsync(b => b.Id == id_backup);

            if (backup == null)
                throw new Exception("El backup no existe");

            var solicitud = new SolicitudRestauracion
            {
                id_supervisor = id_supervisor,
                id_backup = id_backup,
                fecha_solicitud = DateTime.Now,
                fecha_backup = backup.Fecha,
                estado = "Pendiente",
                motivo_solicitud = motivo
            };

            _context.SolicitudRestauraciones.Add(solicitud);
            await _context.SaveChangesAsync();

            return solicitud.id_solicitud;
        }

        public async Task<List<SolicitudRestauracionVM>> ObtenerSolicitudesPendientes()
        {
            try
            {
                var solicitudes = await _context.SolicitudRestauraciones
                    .Where(s => s.estado == "Pendiente")
                    .OrderByDescending(s => s.fecha_solicitud)
                    .ToListAsync();

                if (!solicitudes.Any())
                    return new List<SolicitudRestauracionVM>();

                var supervisorIds = solicitudes.Select(s => s.id_supervisor).Distinct().ToList();
                var backupIds = solicitudes.Select(s => s.id_backup).Distinct().ToList();

                var supervisores = await _context.Usuarios
                    .Where(u => supervisorIds.Contains(u.id_usuario))
                    .Select(u => new { u.id_usuario, u.Nombre, u.apellido })
                    .ToDictionaryAsync(u => u.id_usuario);

                var backups = await _context.Backups
                    .Where(b => backupIds.Contains(b.Id))
                    .Select(b => new { b.Id, b.NombreArchivo, b.RutaArchivo })
                    .ToDictionaryAsync(b => b.Id);

                var resultado = solicitudes.Select(s =>
                {
                    string nombreSupervisor = "N/A";
                    if (supervisores.TryGetValue(s.id_supervisor, out var supervisor))
                    {
                        nombreSupervisor = $"{supervisor.Nombre ?? ""} {supervisor.apellido ?? ""}".Trim();
                        if (string.IsNullOrWhiteSpace(nombreSupervisor))
                            nombreSupervisor = "Sin nombre";
                    }

                    backups.TryGetValue(s.id_backup, out var backup);

                    return new SolicitudRestauracionVM
                    {
                        id_solicitud = s.id_solicitud,
                        id_supervisor = s.id_supervisor,
                        nombre_supervisor = nombreSupervisor,
                        nombre_archivo_backup = backup?.NombreArchivo ?? "N/A",
                        fecha_backup = s.fecha_backup,
                        ruta_archivo = backup?.RutaArchivo ?? "",
                        fecha_solicitud = s.fecha_solicitud,
                        estado = s.estado ?? "Pendiente",
                        motivo_solicitud = s.motivo_solicitud ?? ""
                    };
                }).ToList();

                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener solicitudes pendientes: {ex.Message}", ex);
            }
        }

        public async Task<bool> ProcesarSolicitud(int id_solicitud, int id_gerente, bool aprobar, string observaciones)
        {
            var solicitud = await _context.SolicitudRestauraciones
                .FirstOrDefaultAsync(s => s.id_solicitud == id_solicitud);

            if (solicitud == null)
                throw new Exception("La solicitud no existe");

            solicitud.id_gerente = id_gerente;
            solicitud.observaciones_gerente = observaciones;
            solicitud.fecha_aprobacion = DateTime.Now;

            if (aprobar)
            {
                solicitud.estado = "Aprobada";
            }
            else
            {
                solicitud.estado = "Rechazada";
            }

            _context.SolicitudRestauraciones.Update(solicitud);
            await _context.SaveChangesAsync();

            return aprobar;
        }

        public async Task<bool> RestaurarBaseDatos(int id_solicitud, string connectionString)
        {
            var solicitud = await _context.SolicitudRestauraciones
                .Include(s => s.Backup)
                .FirstOrDefaultAsync(s => s.id_solicitud == id_solicitud);

            if (solicitud == null)
                throw new Exception("La solicitud no existe");

            if (solicitud.estado != "Aprobada")
                throw new Exception("La solicitud no ha sido aprobada");

            try
            {
                solicitud.estado = "EnRestauracion";
                _context.SolicitudRestauraciones.Update(solicitud);
                await _context.SaveChangesAsync();

                await EjecutarRestauracion(solicitud.Backup.RutaArchivo, connectionString);

                solicitud.estado = "Completada";
                solicitud.fecha_restauracion = DateTime.Now;
                _context.SolicitudRestauraciones.Update(solicitud);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                solicitud.estado = "Aprobada";
                solicitud.observaciones_gerente += $" | Error: {ex.Message}";
                _context.SolicitudRestauraciones.Update(solicitud);
                await _context.SaveChangesAsync();
                throw;
            }
        }

        public async Task<List<BackupDTO>> ObtenerBackupsDisponibles()
        {
            try
            {
                var backups = await _context.Backups
                    .Where(b => b.Fecha != null)
                    .OrderByDescending(b => b.Fecha)
                    .ToListAsync();

                return backups.Select(b => new BackupDTO
                {
                    id = b.Id,
                    nombre_archivo = b.NombreArchivo ?? "Sin nombre",
                    fecha = b.Fecha ?? DateTime.MinValue,
                    ruta_archivo = b.RutaArchivo ?? ""
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener backups disponibles: {ex.Message}", ex);
            }
        }

        public async Task<List<SolicitudRestauracionVM>> ObtenerSolicitudesPorSupervisor(int id_supervisor)
        {
            try
            {
                var solicitudes = await _context.SolicitudRestauraciones
                    .Where(s => s.id_supervisor == id_supervisor)
                    .OrderByDescending(s => s.fecha_solicitud)
                    .ToListAsync();

                if (!solicitudes.Any())
                    return new List<SolicitudRestauracionVM>();

                var backupIds = solicitudes.Select(s => s.id_backup).Distinct().ToList();
                var gerenteIds = solicitudes.Where(s => s.id_gerente.HasValue)
                                           .Select(s => s.id_gerente.Value)
                                           .Distinct()
                                           .ToList();

                var backups = await _context.Backups
                    .Where(b => backupIds.Contains(b.Id))
                    .Select(b => new { b.Id, b.NombreArchivo, b.RutaArchivo })
                    .ToDictionaryAsync(b => b.Id);

                var gerentes = await _context.Usuarios
                    .Where(u => gerenteIds.Contains(u.id_usuario))
                    .Select(u => new { u.id_usuario, u.Nombre, u.apellido })
                    .ToDictionaryAsync(u => u.id_usuario);

                var resultado = solicitudes.Select(s =>
                {
                    backups.TryGetValue(s.id_backup, out var backup);

                    string nombreGerente = "Sin asignar";
                    if (s.id_gerente.HasValue && gerentes.TryGetValue(s.id_gerente.Value, out var gerente))
                    {
                        nombreGerente = $"{gerente.Nombre ?? ""} {gerente.apellido ?? ""}".Trim();
                        if (string.IsNullOrWhiteSpace(nombreGerente))
                            nombreGerente = "Sin nombre";
                    }

                    return new SolicitudRestauracionVM
                    {
                        id_solicitud = s.id_solicitud,
                        nombre_archivo_backup = backup?.NombreArchivo ?? "N/A",
                        ruta_archivo = backup?.RutaArchivo ?? "",
                        fecha_backup = s.fecha_backup,
                        fecha_solicitud = s.fecha_solicitud,
                        estado = s.estado ?? "Desconocido",
                        motivo_solicitud = s.motivo_solicitud ?? "",
                        nombre_gerente = nombreGerente,
                        observaciones_gerente = s.observaciones_gerente ?? ""
                    };
                }).ToList();

                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener solicitudes del supervisor: {ex.Message}", ex);
            }
        }

        private async Task EjecutarRestauracion(string rutaBackup, string connectionString)
        {
            await Task.Delay(1000);
        }
    }

    public class BackupDTO
    {
        public int id { get; set; }
        public string nombre_archivo { get; set; }
        public DateTime fecha { get; set; }
        public string ruta_archivo { get; set; }
    }

    public class SolicitudRestauracionVM
    {
        public int id_solicitud { get; set; }
        public int id_supervisor { get; set; }
        public string nombre_supervisor { get; set; }
        public string nombre_archivo_backup { get; set; }
        public string ruta_archivo { get; set; }
        public DateTime? fecha_solicitud { get; set; }
        public DateTime? fecha_backup { get; set; }
        public string estado { get; set; }
        public string nombre_gerente { get; set; }
        public string motivo_solicitud { get; set; }
        public string observaciones_gerente { get; set; }
    }
}