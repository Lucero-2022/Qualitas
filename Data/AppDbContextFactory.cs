using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Qualitas.Data;



namespace SistemaCobranzaProduccionReserva.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("Server=HP_OMEN;Database=Qualitas;User Id=sa;Password=Lucero129507.;TrustServerCertificate=True;");

        return new AppDbContext(optionsBuilder.Options);
        }
    }
}