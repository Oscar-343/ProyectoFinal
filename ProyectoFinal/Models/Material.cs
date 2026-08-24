// TODO: descomentar y ajustar los [Column(...)] cuando se cree la tabla "material" en MySQL.
// Se espera algo como: id_material, nombre, precio_unitario, medida (ej. "gramos", "metros").

/*
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinal.Models
{
    [Table("material")]
    public class Material
    {
        [Key]
        [Column("id_material")]
        public int IdMaterial { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; }

        [Column("precio_unitario", TypeName = "decimal(8,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column("medida")]
        public string Medida { get; set; } // "gramos", "metros", "unidad", etc.
    }
}
*/