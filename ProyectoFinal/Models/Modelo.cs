using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinal.Models
{
    [Table("modelo")]
    public class Modelo
    {
        // Porcentaje que se usa para calcular el precio de venta a partir del costo.
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

        // Se calcula en el controlador sumando el costo de los materiales usados,
        [Column("costo", TypeName = "decimal(8,2)")]
        public decimal Costo { get; set; }

        // Se calcula en el controlador (Costo * PORCENTAJE_GANANCIA),
        [Column("precio_venta", TypeName = "decimal(8,2)")]
        public decimal PrecioVenta { get; set; }

        // Utilidad calculada automáticamente, no se guarda en la base de datos (NotMapped).
        [NotMapped]
        public decimal Utilidad => PrecioVenta - Costo;

        public ICollection<ModeloMaterial> ModeloMateriales { get; set; } = new List<ModeloMaterial>();
    }
}