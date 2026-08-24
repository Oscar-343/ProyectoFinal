using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinal.Models
{
    // Tabla intermedia: conecta un Modelo con los materiales que usa y la cantidad de cada uno.
    [Table("modelo_material")]
    public class ModeloMaterial
    {
        [Column("id_modelo")]
        public int IdModelo { get; set; }

        [ForeignKey("IdModelo")]
        public Modelo Modelo { get; set; }

        [Column("id_material")]
        public int IdMaterial { get; set; }

        [ForeignKey("IdMaterial")]
        public material Material { get; set; }

        [Column("cantidad_utilizada", TypeName = "decimal(8,2)")]
        public decimal Cantidad { get; set; }
    }
}