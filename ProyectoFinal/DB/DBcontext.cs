using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Models;

namespace ProyectoFinal.Data
{
    public class TiendaDbContext : DbContext
    {
        public TiendaDbContext(DbContextOptions<TiendaDbContext> options)
            : base(options)
        {
        }

        public DbSet<material> Material { get; set; }
        public DbSet<Modelo> Modelo { get; set;} 
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<ModeloMaterial> ModeloMaterial { get; set; }
        // Agreguen aquí las entidades que falten

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModeloMaterial>()
                .HasKey(mm => new { mm.IdModelo, mm.IdMaterial });
            base.OnModelCreating(modelBuilder);

            // Aquí van configuraciones extra
        }
    }
}