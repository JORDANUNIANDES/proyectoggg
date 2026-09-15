using Microsoft.EntityFrameworkCore;
using RestaurantManagement.API.Models;

namespace RestaurantManagement.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Plato> Platos { get; set; } = null!;
        public DbSet<Mesa> Mesas { get; set; } = null!;
        public DbSet<Pedido> Pedidos { get; set; } = null!;
        public DbSet<DetallePedido> DetallesPedido { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cliente Configuration
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Cedula).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Cedula).IsUnique();
                entity.Property(e => e.Email).HasMaxLength(150);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Direccion).HasMaxLength(250);
            });

            // Plato Configuration
            modelBuilder.Entity<Plato>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Precio).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Categoria).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
            });

            // Mesa Configuration
            modelBuilder.Entity<Mesa>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Numero).IsRequired();
                entity.HasIndex(e => e.Numero).IsUnique();
                entity.Property(e => e.Capacidad).IsRequired();
                entity.Property(e => e.Ubicacion).HasMaxLength(100);
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(50);
            });

            // Pedido Configuration
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Observaciones).HasMaxLength(500);

                // Relaciones con DeleteBehavior.Restrict para preservar datos históricos
                entity.HasOne(e => e.Cliente)
                      .WithMany(c => c.Pedidos)
                      .HasForeignKey(e => e.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Mesa)
                      .WithMany(m => m.Pedidos)
                      .HasForeignKey(e => e.MesaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // DetallePedido Configuration
            modelBuilder.Entity<DetallePedido>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Pedido)
                      .WithMany(p => p.Detalles)
                      .HasForeignKey(e => e.PedidoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Plato)
                      .WithMany(p => p.DetallesPedido)
                      .HasForeignKey(e => e.PlatoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
