using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Dto;

namespace ProyectoFinal.Service
{
    public class ReporteService : IReporteService
    {
        private readonly TiendaDbContext _context;

        public ReporteService(TiendaDbContext context)
        {
            _context = context;
        }

        public async Task<ReporteDto> GenerarReporteAsync(DateTime desde, DateTime hasta)
        {
            // === 1. Pedidos normales del rango ===
            var pedidosEnRango = await _context.Pedido
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Modelo)
                        .ThenInclude(m => m.ModeloMateriales)
                            .ThenInclude(mm => mm.Material)
                .Where(p => p.FechaPedido >= desde && p.FechaPedido <= hasta)
                .ToListAsync();

            // === 2. Pedidos personalizados del rango ===
            var personalizadosEnRango = await _context.PedidoPersonalizado
                .Include(pp => pp.Materiales)
                    .ThenInclude(m => m.Material)
                .Where(pp => pp.FechaPedido >= desde && pp.FechaPedido <= hasta)
                .ToListAsync();

            var detalles = pedidosEnRango.SelectMany(p => p.Detalles).ToList();

            // === Ingresos ===
            var ingresosNormales = pedidosEnRango.Sum(p => p.PrecioVentaTotal);
            var ingresosPersonalizados = personalizadosEnRango.Sum(pp => pp.PrecioVenta);
            var ingresos = ingresosNormales + ingresosPersonalizados;

            // === Gasto en materiales ===
            var gastoMaterialesNormales = detalles.Sum(d =>
                d.Modelo == null ? 0 :
                d.Cantidad * d.Modelo.ModeloMateriales.Sum(mm => mm.Cantidad * (mm.Material?.PrecioUnitario ?? 0)));

            var gastoMaterialesPersonalizados = personalizadosEnRango.Sum(pp =>
                pp.Materiales.Sum(m => m.Cantidad * (m.Material?.PrecioUnitario ?? 0)));

            var gastoMateriales = gastoMaterialesNormales + gastoMaterialesPersonalizados;

            var utilidad = ingresos - gastoMateriales;
            var margen = ingresos > 0 ? (utilidad / ingresos) * 100 : 0;

            var totalPedidos = pedidosEnRango.Count + personalizadosEnRango.Count;
            var ticketPromedio = totalPedidos > 0 ? ingresos / totalPedidos : 0;

            var horasTrabajadas = pedidosEnRango.Sum(p => p.HorasProduccionTotal)
                                 + personalizadosEnRango.Sum(pp => pp.TiempoProduccion);
            var utilidadPorHora = horasTrabajadas > 0 ? utilidad / horasTrabajadas : 0;

            // === Ranking de modelos (incluye personalizados agrupados por NombreReferencia) ===
            var ventasPorModeloNormal = detalles
                .Where(d => d.Modelo != null)
                .GroupBy(d => d.Modelo.Nombre)
                .Select(g => new
                {
                    Modelo = g.Key,
                    Cantidad = g.Sum(d => d.Cantidad),
                    Ingreso = g.Sum(d => d.Subtotal)
                });

            var ventasPorModeloPersonalizado = personalizadosEnRango
                .GroupBy(pp => pp.NombreReferencia + " (Personalizado)")
                .Select(g => new
                {
                    Modelo = g.Key,
                    Cantidad = g.Count(),
                    Ingreso = g.Sum(pp => pp.PrecioVenta)
                });

            var ventasPorModelo = ventasPorModeloNormal
                .Concat(ventasPorModeloPersonalizado)
                .ToList();

            var masVendido = ventasPorModelo.OrderByDescending(g => g.Cantidad).FirstOrDefault();
            var menosVendido = ventasPorModelo.OrderBy(g => g.Cantidad).FirstOrDefault();
            var masRentable = ventasPorModelo.OrderByDescending(g => g.Ingreso).FirstOrDefault();

            // === Periodo anterior ===
            var duracion = hasta - desde;
            var desdeAnterior = desde - duracion;
            var hastaAnterior = desde.AddDays(-1);

            var pedidosAnterior = await _context.Pedido
                .Include(p => p.Detalles)
                .Where(p => p.FechaPedido >= desdeAnterior && p.FechaPedido <= hastaAnterior)
                .ToListAsync();

            var personalizadosAnterior = await _context.PedidoPersonalizado
                .Where(pp => pp.FechaPedido >= desdeAnterior && pp.FechaPedido <= hastaAnterior)
                .ToListAsync();

            var utilidadAnterior = pedidosAnterior.Sum(p => p.PrecioVentaTotal)
                                  + personalizadosAnterior.Sum(pp => pp.PrecioVenta)
                                  - 0m; // (mismo pendiente de costo si quieres afinar el periodo anterior también)

            var variacion = utilidadAnterior != 0
                ? ((utilidad - utilidadAnterior) / Math.Abs(utilidadAnterior)) * 100
                : 0;

            // === Detalle unificado para la tabla del Excel ===
            var listaPedidosNormales = pedidosEnRango.Select(p => new PedidoReporteItem
            {
                IdPedido = p.IdPedido,
                Cliente = p.Cliente,
                FechaPedido = p.FechaPedido,
                FechaEntrega = p.FechaEntrega,
                Estado = p.Estado.ToString(),
                Total = p.PrecioVentaTotal,
                Horas = p.HorasProduccionTotal,
                Tipo = "Normal"
            });

            var listaPedidosPersonalizados = personalizadosEnRango.Select(pp => new PedidoReporteItem
            {
                IdPedido = pp.IdPedidoPersonalizado,
                Cliente = pp.Cliente,
                FechaPedido = pp.FechaPedido,
                FechaEntrega = pp.FechaEntrega,
                Estado = pp.Estado.ToString(),
                Total = pp.PrecioVenta,
                Horas = pp.TiempoProduccion,
                Tipo = "Personalizado"
            });

            var listaPedidos = listaPedidosNormales
                .Concat(listaPedidosPersonalizados)
                .OrderBy(p => p.FechaPedido)
                .ToList();

            return new ReporteDto
            {
                FechaInicio = desde,
                FechaFin = hasta,

                ModeloMasVendido = masVendido?.Modelo ?? "N/A",
                CantidadMasVendido = masVendido?.Cantidad ?? 0,
                ModeloMenosVendido = menosVendido?.Modelo ?? "N/A",
                CantidadMenosVendido = menosVendido?.Cantidad ?? 0,
                ModeloMasRentable = masRentable?.Modelo ?? "N/A",
                UtilidadModeloMasRentable = masRentable?.Ingreso ?? 0,

                IngresosTotales = ingresos,
                GastoMateriales = gastoMateriales,
                UtilidadTotal = utilidad,
                MargenUtilidad = margen,

                HorasTrabajadas = horasTrabajadas,
                UtilidadPorHora = utilidadPorHora,
                PedidosRealizados = totalPedidos,
                TicketPromedio = ticketPromedio,

                UtilidadPeriodoAnterior = utilidadAnterior,
                VariacionUtilidadPorcentaje = variacion,

                Pedidos = listaPedidos
            };
        }
    }
}