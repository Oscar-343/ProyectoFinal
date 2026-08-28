namespace ProyectoFinal.Dtos
{
    // Representa una línea de selección que llega desde el carrito del catálogo:
    // qué modelo y cuántas unidades quiere el cliente.
    public class SeleccionModeloDto
    {
        public int IdModelo { get; set; }
        public int Cantidad { get; set; }
    }
}