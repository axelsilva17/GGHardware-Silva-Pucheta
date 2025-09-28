using GGHardware.Models;
using Microsoft.EntityFrameworkCore;

namespace GGHardware.Data
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets existentes
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Venta> Venta { get; set; }
        public DbSet<Producto> Producto { get; set; }

        // NUEVOS DbSets para comprobantes
        public DbSet<TipoComprobante> TiposComprobante { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<DetalleVenta> DetalleVenta { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=GGHardware;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones específicas para las relaciones

            // Configurar relación Venta -> TipoComprobante
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.TipoComprobante)
                .WithMany()
                .HasForeignKey(v => v.IdTipoComprobante)
                .OnDelete(DeleteBehavior.SetNull);

            // Configurar relación Venta -> Cliente
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany()
                .HasForeignKey(v => v.id_Cliente)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurar relación Venta -> Usuario
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Usuario)
                .WithMany()
                .HasForeignKey(v => v.id_Usuario)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurar relación DetalleVenta -> Venta
            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(d => d.id_venta)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar relación DetalleVenta -> Producto
            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.id_producto)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurar precisión para campos decimales
            modelBuilder.Entity<DetalleVenta>()
                .Property(d => d.precio_unitario)
                .HasPrecision(10, 2);

            modelBuilder.Entity<DetalleVenta>()
                .Property(d => d.precio_con_descuento)
                .HasPrecision(10, 2);

            // Configurar índices únicos
            modelBuilder.Entity<TipoComprobante>()
                .HasIndex(t => t.codigo)
                .IsUnique();
        }
    }
}