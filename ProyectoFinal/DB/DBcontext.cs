using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Models;

namespace ProyectoFinal.Data
{
    // Cconexión a la base de datos y las tablas que EF Core puede manejar.
    public class TiendaDbContext : DbContext
    {
        public TiendaDbContext(DbContextOptions<TiendaDbContext> options)
            : base(options)
        {
        }

        // Cada DbSet representa una tabla de la base de datos.
        public DbSet<material> Material { get; set; }
        public DbSet<Modelo> Modelo { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<ModeloMaterial> ModeloMaterial { get; set; }
        public DbSet<Pedido> Pedido { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalle { get; set; }
        public DbSet<PedidoPersonalizado> PedidoPersonalizado { get; set; }
        public DbSet<PedidoPersonalizadoMaterial> PedidoPersonalizadoMaterial { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Llave primaria es la combinación de IdModelo + IdMaterial (llave compuesta).
            modelBuilder.Entity<ModeloMaterial>()
                .HasKey(mm => new { mm.IdModelo, mm.IdMaterial });

            base.OnModelCreating(modelBuilder);
        }
    }
}