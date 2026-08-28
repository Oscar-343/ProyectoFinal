using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinal.Models
{
    [Table("pedido_detalle")]
    public class PedidoDetalle
    {
        [Key]
        [Column("id_pedido_detalle")]
        public int IdPedidoDetalle { get; set; }

        [Column("id_pedido")]
        public int IdPedido { get; set; }

        [ForeignKey(nameof(IdPedido))]
        public Pedido? Pedido { get; set; }

        [Column("id_modelo")]
        public int IdModelo { get; set; }

        [ForeignKey(nameof(IdModelo))]
        public Modelo? Modelo { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [NotMapped]
        public decimal Subtotal => Cantidad * (Modelo?.PrecioVenta ?? 0);
    }
}