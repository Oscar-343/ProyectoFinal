using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinal.Models
{
    // Esta clase representa una obra/amigurumi registrado en el sistema (tabla "modelo").
    [Table("modelo")]
    public class Modelo
    {
        // Porcentaje de ganancia "de respaldo", solo se usa si la dificultad
        // no coincide con "baja", "media" ni "alta" (caso raro, por seguridad).
        public const decimal PORCENTAJE_GANANCIA = 1.5m;

        [Key] // Marca este campo como la llave primaria (identificador único) del modelo.
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

        // Nivel de dificultad de la obra: "Baja", "Media" o "Alta".
        // Este valor se usa en el controller para decidir cuánto margen de ganancia aplicar.
        [Required(ErrorMessage = "La dificultad es obligatoria.")]
        [Column("dificultad")]
        public string Dificultad { get; set; }

        // Horas que toma producir la obra. Se usa para calcular la mano de obra (horas x Bs. 5).
        [Required(ErrorMessage = "El tiempo de producción es obligatorio.")]
        [Column("tiempo_produccion", TypeName = "decimal(5,2)")]
        public decimal TiempoProduccion { get; set; }

        // Costo total = materiales usados + mano de obra. Se calcula automáticamente
        // en el controller, el usuario no lo escribe a mano.
        [Column("costo", TypeName = "decimal(8,2)")]
        public decimal Costo { get; set; }

        // Precio final de venta = Costo x multiplicador según dificultad.
        // También se calcula automáticamente en el controller.
        [Column("precio_venta", TypeName = "decimal(8,2)")]
        public decimal PrecioVenta { get; set; }

        // [NotMapped] significa que esta propiedad NO existe como columna en la base de datos;
        // se calcula al vuelo en memoria cada vez que se usa (precio de venta - costo).
        [NotMapped]
        public decimal Utilidad => PrecioVenta - Costo;

        // Lista de materiales usados en esta obra, con su cantidad (relación uno a muchos).
        public ICollection<ModeloMaterial> ModeloMateriales { get; set; } = new List<ModeloMaterial>();
    }
}