using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MuscleHouse.Models;

namespace MuscleHouse.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Entrenador> Entrenadores { get; set; } = null!;
        public DbSet<Recepcionista> Recepcionistas { get; set; } = null!;
        public DbSet<AsignacionEntrenador> AsignacionesEntrenadores { get; set; } = null!;
        public DbSet<Plan> Planes { get; set; } = null!;
        public DbSet<Membresia> Membresias { get; set; } = null!;
        public DbSet<Pago> Pagos { get; set; } = null!;
        public DbSet<Asistencia> Asistencias { get; set; } = null!;
        public DbSet<Ejercicio> Ejercicios { get; set; } = null!;
        public DbSet<Rutina> Rutinas { get; set; } = null!;
        public DbSet<RutinaEjercicio> RutinaEjercicios { get; set; } = null!;
        public DbSet<Progreso> Progresos { get; set; } = null!;
        public DbSet<RegistroEntrenamiento> RegistrosEntrenamiento { get; set; } = null!;
        public DbSet<Notificacion> Notificaciones { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Identity -> Cliente
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Identity -> Entrenador
            modelBuilder.Entity<Entrenador>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Identity -> Recepcionista
            modelBuilder.Entity<Recepcionista>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. Cliente -> Entrenador (Current Trainer)
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Entrenador)
                .WithMany(e => e.Clientes)
                .HasForeignKey(c => c.EntrenadorId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. AsignacionEntrenador Relations
            modelBuilder.Entity<AsignacionEntrenador>()
                .HasOne(ae => ae.Cliente)
                .WithMany(c => c.AsignacionesEntrenadores)
                .HasForeignKey(ae => ae.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AsignacionEntrenador>()
                .HasOne(ae => ae.Entrenador)
                .WithMany(e => e.AsignacionesClientes)
                .HasForeignKey(ae => ae.EntrenadorId)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. Membresia Relations
            modelBuilder.Entity<Membresia>()
                .HasOne(m => m.Cliente)
                .WithMany(c => c.Membresias)
                .HasForeignKey(m => m.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Membresia>()
                .HasOne(m => m.Plan)
                .WithMany(p => p.Membresias)
                .HasForeignKey(m => m.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // 7. Pago Relations
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Pagos)
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict); // Avoid Multiple Cascade Paths (Cliente -> Membresia -> Pago and Cliente -> Pago)

            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Membresia)
                .WithMany(m => m.Pagos)
                .HasForeignKey(p => p.MembresiaId)
                .OnDelete(DeleteBehavior.Cascade);

            // 8. Asistencia Relations
            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.Cliente)
                .WithMany(c => c.Asistencias)
                .HasForeignKey(a => a.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // 9. Progreso Relations
            modelBuilder.Entity<Progreso>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Progresos)
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // 10. Rutina Relations
            modelBuilder.Entity<Rutina>()
                .HasOne(r => r.Cliente)
                .WithMany(c => c.Rutinas)
                .HasForeignKey(r => r.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Rutina>()
                .HasOne(r => r.Entrenador)
                .WithMany(e => e.Rutinas)
                .HasForeignKey(r => r.EntrenadorId)
                .OnDelete(DeleteBehavior.Restrict);

            // 11. RutinaEjercicio Relations (Many-to-Many join table)
            modelBuilder.Entity<RutinaEjercicio>()
                .HasOne(re => re.Rutina)
                .WithMany(r => r.RutinaEjercicios)
                .HasForeignKey(re => re.RutinaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RutinaEjercicio>()
                .HasOne(re => re.Ejercicio)
                .WithMany(e => e.RutinasEjercicios)
                .HasForeignKey(re => re.EjercicioId)
                .OnDelete(DeleteBehavior.Restrict);

            // 12. RegistroEntrenamiento Relations
            modelBuilder.Entity<RegistroEntrenamiento>()
                .HasOne(re => re.Cliente)
                .WithMany(c => c.RegistrosEntrenamiento)
                .HasForeignKey(re => re.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RegistroEntrenamiento>()
                .HasOne(re => re.Ejercicio)
                .WithMany(e => e.RegistrosEntrenamiento)
                .HasForeignKey(re => re.EjercicioId)
                .OnDelete(DeleteBehavior.Restrict);

            // 13. Notificacion Relations
            modelBuilder.Entity<Notificacion>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Decimal property precision settings for SQL Server
            modelBuilder.Entity<Plan>()
                .Property(p => p.Precio)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Membresia>()
                .Property(m => m.PrecioPagado)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Pago>()
                .Property(p => p.Monto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RutinaEjercicio>()
                .Property(re => re.PesoRecomendado)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Progreso>()
                .Property(p => p.Peso).HasPrecision(18, 2);
            modelBuilder.Entity<Progreso>()
                .Property(p => p.Pecho).HasPrecision(18, 2);
            modelBuilder.Entity<Progreso>()
                .Property(p => p.Cintura).HasPrecision(18, 2);
            modelBuilder.Entity<Progreso>()
                .Property(p => p.Brazo).HasPrecision(18, 2);
            modelBuilder.Entity<Progreso>()
                .Property(p => p.Pierna).HasPrecision(18, 2);
            modelBuilder.Entity<Progreso>()
                .Property(p => p.Cadera).HasPrecision(18, 2);

            modelBuilder.Entity<RegistroEntrenamiento>()
                .Property(re => re.Peso).HasPrecision(18, 2);
        }
    }
}
