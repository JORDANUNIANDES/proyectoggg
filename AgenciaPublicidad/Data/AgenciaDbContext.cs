using Microsoft.EntityFrameworkCore;
using AgenciaPublicidad.Models;

namespace AgenciaPublicidad.Data
{
    public class AgenciaDbContext : DbContext
    {
        public AgenciaDbContext(DbContextOptions<AgenciaDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Campana> Campanas { get; set; }
        public DbSet<Disenador> Disenadores { get; set; }
        public DbSet<Entregable> Entregables { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relaciones:
            // Un Cliente puede tener muchas Campañas.
            // Una Campaña pertenece a un Cliente.
            modelBuilder.Entity<Campana>()
                .HasOne(c => c.Cliente)
                .WithMany(cl => cl.Campanas)
                .HasForeignKey(c => c.cliente_id)
                .OnDelete(DeleteBehavior.Cascade);

            // Un Entregable pertenece a una Campaña.
            modelBuilder.Entity<Entregable>()
                .HasOne(e => e.Campana)
                .WithMany(c => c.Entregables)
                .HasForeignKey(e => e.campana_id)
                .OnDelete(DeleteBehavior.Cascade);

            // Un Entregable es asignado a un Diseñador.
            modelBuilder.Entity<Entregable>()
                .HasOne(e => e.Disenador)
                .WithMany(d => d.Entregables)
                .HasForeignKey(e => e.disenador_id)
                .OnDelete(DeleteBehavior.Restrict); // No cascade on Disenador delete to avoid multi-path cascade cycles in SQL Server.
        }
    }
}
