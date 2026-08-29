using GestionClientes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionClientes.Infrastructure.Data;

public class ClientesDbContext : DbContext
{
    public ClientesDbContext(DbContextOptions<ClientesDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClientesDbContext).Assembly);
    }
}
