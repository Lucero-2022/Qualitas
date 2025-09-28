using Microsoft.EntityFrameworkCore;
using Qualitas.Models;


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; }

    public DbSet<Cobranza> Cobranzas { get; set; }

    public DbSet<Reserva> Reservas { get; set; }

    public DbSet<Produccion> Producciones { get; set; }

    
    

    

}
