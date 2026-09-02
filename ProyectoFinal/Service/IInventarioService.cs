namespace ProyectoFinal.Services
{
    public interface IInventarioService
    {
        // Descuenta del stock los materiales usados por un pedido de catálogo (normal).
        // idsDetalles: lista de (idModelo, cantidad) del pedido ya guardado.
        Task DescontarStockPedidoNormalAsync(int idPedido);

        // Descuenta del stock los materiales usados por un pedido personalizado.
        Task DescontarStockPedidoPersonalizadoAsync(int idPedidoPersonalizado);
    }
}
