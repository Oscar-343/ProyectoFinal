using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinal.Models
{
    // Esta clase representa la tabla intermedia "modelo_material".
    // Sirve para saber QUÉ materiales y en QUÉ cantidad se usaron en cada modelo (obra).
    // Un modelo puede tener varios materiales, y un material puede usarse en varios modelos.
    [Table("modelo_material")]
    public class ModeloMaterial
    {
        // Guarda a qué modelo (obra) pertenece este registro.
        [Column("id_modelo")]
        public int IdModelo { get; set; }

        // Le dice a Entity Framework: "IdModelo es la llave que conecta con la clase Modelo".
        // Sin esto, EF no sabe cómo relacionar ambas tablas y falla al guardar.
        [ForeignKey("IdModelo")]
        public Modelo Modelo { get; set; }

        // Guarda qué material se usó (ej: lana, ojos de seguridad, relleno, etc.)
        [Column("id_material")]
        public int IdMaterial { get; set; }

        // Le dice a Entity Framework: "IdMaterial es la llave que conecta con la clase material".
        // Igual que arriba, sin esto EF se confunde y crea una columna que no existe.
        [ForeignKey("IdMaterial")]
        public material Material { get; set; }

        // Cuánto se usó de ese material para este modelo (ej: 2.5 ovillos de lana).
        // Se guarda en la columna "cantidad_utilizada" de la base de datos.
        [Column("cantidad_utilizada", TypeName = "decimal(8,2)")]
        public decimal Cantidad { get; set; }
    }
}