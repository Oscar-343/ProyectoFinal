using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinal.Models
{
    [Table("pedidos")]
    public class Pedido
    {
        [Key]
        [Column("id_pedido")]
        public int IdPedido { get; set; }

        [Required(ErrorMessage = "El cliente es obligatorio.")]
        [StringLength(150)]
        [Column("cliente")]
        public string Cliente { get; set; } = string.Empty;

        // Fecha en la que se registra el pedido en el sistema.
        [Column("fecha_pedido")]
        public DateTime FechaPedido { get; set; } = DateTime.Now;

        // Fecha/hora real en la que arranca la producción (ya ajustada a la cola y al horario laboral).
        [Column("fecha_inicio")]
        public DateTime FechaInicio { get; set; }

        // Fecha/hora calculada de entrega.
        [Column("fecha_entrega")]
        public DateTime FechaEntrega { get; set; }

        [Column("estado")]
        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

        public ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();

        // Precio total del pedido. NO se guarda en BD: se calcula en vivo
        [NotMapped]
        public decimal PrecioVentaTotal => Detalles?.Sum(d => d.Subtotal) ?? 0;

        // Horas totales de producción que representa este pedido.
        [NotMapped]
        public decimal HorasProduccionTotal => Detalles?.Sum(d => d.Modelo != null ? d.Modelo.TiempoProduccion * d.Cantidad : 0) ?? 0;
    }
}