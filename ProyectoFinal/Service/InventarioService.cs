using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;

namespace ProyectoFinal.Services
{
    // Se encarga de descontar del stock los materiales que se consumen
    // cuando un pedido (de catálogo o personalizado) se marca como Entregado.
    public class InventarioService : IInventarioService
    {
        private readonly TiendaDbContext _context;

        public InventarioService(TiendaDbContext context)
        {
            _context = context;
        }

        public async Task DescontarStockPedidoNormalAsync(int idPedido)
        {
            // Trae los detalles del pedido junto con los materiales que usa cada modelo.
            var detalles = await _context.PedidoDetalle
                .Where(d => d.IdPedido == idPedido)
                .Include(d => d.Modelo)
                    .ThenInclude(m => m.ModeloMateriales)
                .ToListAsync();

            foreach (var detalle in detalles)
            {
                if (detalle.Modelo?.ModeloMateriales == null)
                    continue;

                foreach (var modeloMaterial in detalle.Modelo.ModeloMateriales)
                {
                    // Cantidad usada = lo que gasta una unidad del modelo x cuántas unidades se pidieron.
                    decimal cantidadADescontar = modeloMaterial.Cantidad * detalle.Cantidad;
                    await DescontarMaterialAsync(modeloMaterial.IdMaterial, cantidadADescontar);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DescontarStockPedidoPersonalizadoAsync(int idPedidoPersonalizado)
        {
            var materiales = await _context.PedidoPersonalizadoMaterial
                .Where(m => m.IdPedidoPersonalizado == idPedidoPersonalizado)
                .ToListAsync();

            foreach (var item in materiales)
            {
                await DescontarMaterialAsync(item.IdMaterial, item.Cantidad);
            }

            await _context.SaveChangesAsync();
        }

        // Resta la cantidad indicada del stock de un material, sin dejarlo bajar de 0
        // (por si el stock real ya era menor al esperado por algún ajuste manual previo),
        // y recalcula el campo "Estado" (disponible/bajo/agotado) para que el badge
        // en la lista de materiales quede al día automáticamente.
        private async Task DescontarMaterialAsync(int idMaterial, decimal cantidad)
        {
            var material = await _context.Material.FindAsync(idMaterial);

            if (material == null)
                return;

            material.CantidadDisponible = Math.Max(0, material.CantidadDisponible - cantidad);

            if (material.CantidadDisponible <= 0)
                material.Estado = "stock agotado";
            else if (material.CantidadDisponible <= material.StockMinimo)
                material.Estado = "stock bajo";
            else
                material.Estado = "stock disponible";
        }
    }
}
