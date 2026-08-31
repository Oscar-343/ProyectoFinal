using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinal.Models
{
    [Table("material")]
    public class material
    {
        [Key]
        [Column("id_material")]
        public int IdMaterial { get; set; }

        [Required(ErrorMessage = "Debe ingresar el nombre del material")]
        [Column("nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Debe ingresar el tipo de material")]
        [Column("tipo_material")]
        public string Tipo { get; set; }

        [Required(ErrorMessage = "Debe ingresar el color")]
        [Column("color")]
        public string Color { get; set; }

        [Required(ErrorMessage = "Debe ingresar la unidad de medida")]
        [Column("unidad_medida")]
        public string UnidadMedida { get; set; }

        [Column("cantidad_disponible", TypeName = "decimal(10,2)")]
        public decimal CantidadDisponible { get; set; }

        [Column("precio_unitario", TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column("stock_minimo", TypeName = "decimal(10,2)")]
        public decimal StockMinimo { get; set; }

        // Se calcula automáticamente en el controlador,
        // no lo llena el usuario en el formulario.
        [Column("estado")]
        public string? Estado { get; set; } = "stock disponible";
    }
}