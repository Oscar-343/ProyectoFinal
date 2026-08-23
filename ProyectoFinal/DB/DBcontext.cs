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
        public DbSet<modelo> Modelo { get; set; }
        public DbSet<modelo_material> Modelo_Material { get; set; }
        public DbSet<usuario> Usuarios { get; set; }
        // Agreguen aquí las entidades que falten

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aquí van configuraciones extra
        }
    }
}