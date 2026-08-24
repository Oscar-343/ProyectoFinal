using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinal.Models
{
    [Table("modelo_material")]
    public class ModeloMaterial
    {
        [Column("id_modelo")]
        public int IdModelo { get; set; }
        public Modelo Modelo { get; set; }

        [Column("id_material")]
        public int IdMaterial { get; set; }
        public material Material { get; set; }

        [Column("cantidad", TypeName = "decimal(8,2)")]
        public decimal Cantidad { get; set; }
    }
}