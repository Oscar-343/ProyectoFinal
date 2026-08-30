using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinal.Models
{
    [Table("pedido_personalizado_material")]
    public class PedidoPersonalizadoMaterial
    {
        [Key]
        [Column("id_pedido_personalizado_material")]
        public int IdPedidoPersonalizadoMaterial { get; set; }

        [Column("id_pedido_personalizado")]
        public int IdPedidoPersonalizado { get; set; }

        [ForeignKey(nameof(IdPedidoPersonalizado))]
        public PedidoPersonalizado? PedidoPersonalizado { get; set; }

        [Column("id_material")]
        public int IdMaterial { get; set; }

        [ForeignKey(nameof(IdMaterial))]
        public material? Material { get; set; }

        [Column("cantidad", TypeName = "decimal(10,2)")]
        public decimal Cantidad { get; set; }
    }
}