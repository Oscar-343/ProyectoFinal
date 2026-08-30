using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinal.Models
{
    [Table("pedido_personalizado")]
    public class PedidoPersonalizado
    {
        [Key]
        [Column("id_pedido_personalizado")]
        public int IdPedidoPersonalizado { get; set; }

        [Required(ErrorMessage = "El cliente es obligatorio.")]
        [StringLength(150)]
        [Column("cliente")]
        public string Cliente { get; set; } = string.Empty;

        // Nombre corto para identificar el pedido en las listas (ej: "Amigurumi Goku personalizado").
        [Required(ErrorMessage = "Da un nombre de referencia para identificar el pedido.")]
        [StringLength(150)]
        [Column("nombre_referencia")]
        public string NombreReferencia { get; set; } = string.Empty;

        [StringLength(500)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        // Ruta relativa del archivo subido por drag & drop, ej: /uploads/personalizados/xxxx.jpg
        [Column("imagen")]
        public string? Imagen { get; set; }

        // Definidos por el admin al registrar el pedido (no hay catálogo de referencia).
        [Required(ErrorMessage = "La dificultad es obligatoria.")]
        [Column("dificultad")]
        public string Dificultad { get; set; } = "Media";

        [Required(ErrorMessage = "El tiempo de producción estimado es obligatorio.")]
        [Column("tiempo_produccion", TypeName = "decimal(5,2)")]
        public decimal TiempoProduccion { get; set; }

        // Calculados automáticamente: materiales + mano de obra, y precio según dificultad.
        [Column("costo", TypeName = "decimal(8,2)")]
        public decimal Costo { get; set; }

        [Column("precio_venta", TypeName = "decimal(8,2)")]
        public decimal PrecioVenta { get; set; }

        [Column("fecha_pedido")]
        public DateTime FechaPedido { get; set; } = DateTime.Now;

        [Column("fecha_inicio")]
        public DateTime FechaInicio { get; set; }

        [Column("fecha_entrega")]
        public DateTime FechaEntrega { get; set; }

        // Reutiliza el mismo enum que Pedido: comparten la misma línea de producción.
        [Column("estado")]
        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

        public ICollection<PedidoPersonalizadoMaterial> Materiales { get; set; } = new List<PedidoPersonalizadoMaterial>();
    }
}