using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;

namespace ProyectoFinal.Services
{
    // Calcula fechas de producción asumiendo UNA sola línea de producción secuencial.
    // IMPORTANTE: los pedidos normales (Pedido) y los personalizados (PedidoPersonalizado)
    // comparten la MISMA línea física, así que este servicio revisa las dos tablas juntas
    // para saber cuándo se libera la cola. Si solo mirara una tabla, se podrían prometer
    // fechas de entrega que en realidad ya están ocupadas por la otra.
    public class ColaProduccionService : IColaProduccionService
    {
        private readonly TiendaDbContext _context;

        private static readonly TimeSpan HORA_INICIO = new TimeSpan(8, 0, 0);
        private static readonly TimeSpan HORA_FIN = new TimeSpan(20, 0, 0);

        public ColaProduccionService(TiendaDbContext context)
        {
            _context = context;
        }

        // Fecha en la que termina el último pedido en cola (de cualquiera de las dos tablas),
        // considerando solo los que siguen "activos" (pendiente o en producción).
        public DateTime ObtenerFechaFinCola()
        {
            var maxPedidoNormal = _context.Pedido
                .Where(p => p.Estado == EstadoPedido.Pendiente || p.Estado == EstadoPedido.EnProduccion)
                .Select(p => (DateTime?)p.FechaEntrega)
                .Max();

            var maxPedidoPersonalizado = _context.PedidoPersonalizado
                .Where(p => p.Estado == EstadoPedido.Pendiente || p.Estado == EstadoPedido.EnProduccion)
                .Select(p => (DateTime?)p.FechaEntrega)
                .Max();

            var candidatos = new List<DateTime>();
            if (maxPedidoNormal.HasValue) candidatos.Add(maxPedidoNormal.Value);
            if (maxPedidoPersonalizado.HasValue) candidatos.Add(maxPedidoPersonalizado.Value);

            return candidatos.Any() ? candidatos.Max() : DateTime.Now;
        }

        public (DateTime fechaInicioReal, DateTime fechaEntrega) CalcularFechasPedido(DateTime fechaInicioDeseada, decimal horasTotales)
        {
            var finCola = ObtenerFechaFinCola();

            var inicioReal = fechaInicioDeseada > finCola ? fechaInicioDeseada : finCola;
            inicioReal = AjustarAHorarioLaboral(inicioReal);

            var entrega = CalcularFechaEntrega(inicioReal, horasTotales);

            return (inicioReal, entrega);
        }

        private DateTime CalcularFechaEntrega(DateTime fechaInicio, decimal horasTotales)
        {
            var actual = AjustarAHorarioLaboral(fechaInicio);
            var horasRestantes = horasTotales;

            while (horasRestantes > 0)
            {
                var horasDisponiblesHoy = (decimal)(HORA_FIN - actual.TimeOfDay).TotalHours;

                if (horasRestantes <= horasDisponiblesHoy)
                {
                    actual = actual.AddHours((double)horasRestantes);
                    horasRestantes = 0;
                }
                else
                {
                    horasRestantes -= horasDisponiblesHoy;
                    actual = SiguienteDiaLaboral(actual.Date).Add(HORA_INICIO);
                }
            }

            return actual;
        }

        private DateTime AjustarAHorarioLaboral(DateTime fecha)
        {
            if (fecha.DayOfWeek == DayOfWeek.Sunday)
                return SiguienteDiaLaboral(fecha.Date).Add(HORA_INICIO);

            if (fecha.TimeOfDay < HORA_INICIO)
                return fecha.Date.Add(HORA_INICIO);

            if (fecha.TimeOfDay >= HORA_FIN)
                return SiguienteDiaLaboral(fecha.Date).Add(HORA_INICIO);

            return fecha;
        }

        private DateTime SiguienteDiaLaboral(DateTime fecha)
        {
            var siguiente = fecha.AddDays(1);
            while (siguiente.DayOfWeek == DayOfWeek.Sunday)
                siguiente = siguiente.AddDays(1);
            return siguiente;
        }
    }
}