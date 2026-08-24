using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinal.Models
{
    [Table("modelo")]
    public class Modelo
    {
        // Porcentaje de ganancia aplicado sobre el costo (1.5 = 50% de ganancia)
        public const decimal PORCENTAJE_GANANCIA = 1.5m;

        [Key]
        [Column("id_modelo")]
        public int IdModelo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(500)]
        [Column("descripcion")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La imagen de referencia es obligatoria.")]
        [Column("imagen")]
        public string Imagen { get; set; }

        [Required(ErrorMessage = "La dificultad es obligatoria.")]
        [Column("dificultad")]
        public string Dificultad { get; set; }

        [Required(ErrorMessage = "El tiempo de producción es obligatorio.")]
        [Column("tiempo_produccion", TypeName = "decimal(5,2)")]
        public decimal TiempoProduccion { get; set; }

        // Ya no se ingresan a mano: se calculan en el controlador
        // a partir de los materiales seleccionados, y se guardan
        // en la BD para no recalcular cada vez que se lista el catálogo.
        [Column("costo", TypeName = "decimal(8,2)")]
        public decimal Costo { get; set; }

        [Column("precio_venta", TypeName = "decimal(8,2)")]
        public decimal PrecioVenta { get; set; }

        [NotMapped]
        public decimal Utilidad => PrecioVenta - Costo;

        // TODO: descomentar cuando exista la tabla/CRUD de Material.
        // Relación con los materiales usados en este modelo, cada uno con su cantidad.
        // [NotMapped] no aplica aquí porque sí queremos que EF Core la mapee.
        //
        // public ICollection<ModeloMaterial> ModeloMateriales { get; set; } = new List<ModeloMaterial>();
    }
}