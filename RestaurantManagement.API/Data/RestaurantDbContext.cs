using Microsoft.EntityFrameworkCore;
using RestaurantManagement.API.Models;

namespace RestaurantManagement.API.Data
{
    public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Plato> Platos => Set<Plato>();
        public DbSet<Mesa> Mesas => Set<Mesa>();
        public DbSet<Pedido> Pedidos => Set<Pedido>();
        public DbSet<DetallePedido> DetallesPedido => Set<DetallePedido>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Precision for Prices and Totals
            modelBuilder.Entity<Plato>()
                .Property(p => p.Precio)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DetallePedido>()
                .Property(d => d.PrecioUnitario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DetallePedido>()
                .Property(d => d.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Pedido>()
                .Property(p => p.Total)
                .HasPrecision(18, 2);

            // Relationships and Delete Behaviors
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Mesa)
                .WithMany(m => m.Pedidos)
                .HasForeignKey(p => p.MesaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Pedido)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => d.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Plato)
                .WithMany(p => p.DetallesPedido)
                .HasForeignKey(d => d.PlatoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Cedula)
                .IsUnique();

            modelBuilder.Entity<Mesa>()
                .HasIndex(m => m.NumeroMesa)
                .IsUnique();

            // Seed Data
            modelBuilder.Entity<Cliente>().HasData(
                new Cliente { Id = 1, Nombre = "Juan", Apellido = "Perez", Cedula = "1710034065", Telefono = "0991234567", Email = "juan.perez@example.com", Activo = true },
                new Cliente { Id = 2, Nombre = "Maria", Apellido = "Lopez", Cedula = "0926629916", Telefono = "0987654321", Email = "maria.lopez@example.com", Activo = true }
            );

            modelBuilder.Entity<Plato>().HasData(
                new Plato { Id = 1, Nombre = "Encebollado Mixto", Descripcion = "Sopa tradicional de albacora y camarón con yuca y cebolla curtida", Precio = 5.50m, Categoria = "Platos Fuertes", Disponible = true, Activo = true },
                new Plato { Id = 2, Nombre = "Lomo Saltado", Descripcion = "Trozos de lomo de res salteados con cebolla, tomate y papas fritas", Precio = 8.00m, Categoria = "Platos Fuertes", Disponible = true, Activo = true },
                new Plato { Id = 3, Nombre = "Arroz con Camarones", Descripcion = "Arroz marinado sazonado con vegetales y camarones frescos", Precio = 7.50m, Categoria = "Mariscos", Disponible = true, Activo = true },
                new Plato { Id = 4, Nombre = "Jugo Natural de Naranjilla", Descripcion = "Bebida refrescante de fruta natural (500ml)", Precio = 1.50m, Categoria = "Bebidas", Disponible = true, Activo = true }
            );

            modelBuilder.Entity<Mesa>().HasData(
                new Mesa { Id = 1, NumeroMesa = 1, Capacidad = 2, Estado = MesaEstado.Disponible, Activo = true },
                new Mesa { Id = 2, NumeroMesa = 2, Capacidad = 4, Estado = MesaEstado.Disponible, Activo = true },
                new Mesa { Id = 3, NumeroMesa = 3, Capacidad = 6, Estado = MesaEstado.Disponible, Activo = true },
                new Mesa { Id = 4, NumeroMesa = 4, Capacidad = 8, Estado = MesaEstado.Disponible, Activo = true }
            );
        }
    }
}
