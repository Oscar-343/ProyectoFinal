using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinal.Models
{
    // Representa la relación entre modelos y materiales.
    [Table("modelo_material")]
    public class ModeloMaterial
    {
        // Identifica el modelo relacionado.
        [Column("id_modelo")]
        public int IdModelo { get; set; }

        [ForeignKey("IdModelo")]
        public Modelo Modelo { get; set; }

        // Identifica el material relacionado.
        [Column("id_material")]
        public int IdMaterial { get; set; }

        [ForeignKey("IdMaterial")]
        public material Material { get; set; }

        // Cantidad de material utilizada en el modelo.
        [Column("cantidad_utilizada", TypeName = "decimal(8,2)")]
        public decimal Cantidad { get; set; }
    }
}

