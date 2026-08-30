using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ProyectoFinal.Data;
using ProyectoFinal.Dtos;
using ProyectoFinal.Filters;
using ProyectoFinal.Models;
using ProyectoFinal.Services;
using ProyectoFinal.ViewModels;

namespace ProyectoFinal.Controllers
{
    [RequiereSesion]
    public class PedidoController : Controller
    {
        private readonly TiendaDbContext _context;
        private readonly IColaProduccionService _colaProduccion;

        public const string SESSION_KEY_SELECCION = "SeleccionModelos";

        public PedidoController(
            TiendaDbContext context,
            IColaProduccionService colaProduccion)
        {
            _context = context;
            _colaProduccion = colaProduccion;
        }

        // Muestra los pedidos (normales + personalizados) y permite filtrarlos por estado.
        public async Task<IActionResult> Index(EstadoPedido? estado = null)
        {
            // 1. Trae los pedidos normales
            var queryNormales = _context.Pedido
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Modelo)
                .AsQueryable();

            if (estado.HasValue)
                queryNormales = queryNormales.Where(p => p.Estado == estado.Value);

            var normales = await queryNormales.ToListAsync();

            // 2. Trae los pedidos personalizados
            var queryPersonalizados = _context.PedidoPersonalizado.AsQueryable();

            if (estado.HasValue)
                queryPersonalizados = queryPersonalizados.Where(p => p.Estado == estado.Value);

            var personalizados = await queryPersonalizados.ToListAsync();

            // 3. Convierte ambos a un mismo ViewModel
            var listaNormales = normales.Select(p => new PedidoResumenViewModel
            {
                Id = p.IdPedido,
                Tipo = "Normal",
                Cliente = p.Cliente,
                Descripcion = string.Join(", ", p.Detalles.Select(d => $"{d.Modelo?.Nombre} x{d.Cantidad}")),
                FechaPedido = p.FechaPedido,
                FechaInicio = p.FechaInicio,
                FechaEntrega = p.FechaEntrega,
                Estado = p.Estado,
                Total = p.PrecioVentaTotal
            });

            var listaPersonalizados = personalizados.Select(p => new PedidoResumenViewModel
            {
                Id = p.IdPedidoPersonalizado,
                Tipo = "Personalizado",
                Cliente = p.Cliente,
                Descripcion = p.NombreReferencia,
                FechaPedido = p.FechaPedido,
                FechaInicio = p.FechaInicio,
                FechaEntrega = p.FechaEntrega,
                Estado = p.Estado,
                Total = p.PrecioVenta
            });

            // 4. Combina y ordena
            var pedidos = listaNormales
                .Concat(listaPersonalizados)
                .OrderByDescending(p => p.FechaPedido)
                .ToList();

            if (estado.HasValue)
            {
                ViewBag.TituloLista = $"Pedidos - {estado.Value}";
                ViewBag.MostrarTiempoRestante = true;
            }
            else
            {
                ViewBag.TituloLista = "Todos los pedidos";
                ViewBag.MostrarTiempoRestante = false;
            }

            ViewBag.EstadoFiltro = estado;

            return View(pedidos);
        }

        // Muestra el detalle de un pedido.
        public async Task<IActionResult> Details(int id)
        {
            var pedido = await _context.Pedido
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Modelo)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            return View(pedido);
        }

        // Recibe los modelos seleccionados desde el catálogo.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RecibirSeleccion(
            [FromBody] List<SeleccionModeloDto> seleccion)
        {
            if (seleccion == null || !seleccion.Any())
                return BadRequest("Debe seleccionar al menos un modelo.");

            seleccion = seleccion
                .Where(s => s.Cantidad > 0)
                .ToList();

            if (!seleccion.Any())
                return BadRequest("Las cantidades deben ser mayores a cero.");

            HttpContext.Session.SetString(
                SESSION_KEY_SELECCION,
                JsonSerializer.Serialize(seleccion));

            return Ok(new
            {
                redirectUrl = Url.Action("Create", "Pedido")
            });
        }

        // Muestra el formulario para crear un pedido.
        public async Task<IActionResult> Create()
        {
            var seleccion = ObtenerSeleccionDeSesion();

            if (seleccion == null || !seleccion.Any())
            {
                TempData["Error"] =
                    "No hay modelos seleccionados. Vuelve al catálogo y elige al menos uno.";

                return RedirectToAction("Catalogo", "Modelo");
            }

            var vm = await ArmarViewModelAsync(
                seleccion,
                DateTime.Now);

            return View(vm);
        }

        // Registra un nuevo pedido.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PedidoCreateViewModel vm)
        {
            var seleccion = ObtenerSeleccionDeSesion();

            if (seleccion == null || !seleccion.Any())
            {
                TempData["Error"] =
                    "La selección expiró. Vuelve al catálogo y elige los modelos de nuevo.";

                return RedirectToAction("Catalogo", "Modelo");
            }

            if (string.IsNullOrWhiteSpace(vm.Cliente))
                ModelState.AddModelError(
                    nameof(vm.Cliente),
                    "El cliente es obligatorio.");

            if (vm.FechaInicio == default)
                ModelState.AddModelError(
                    nameof(vm.FechaInicio),
                    "Debes elegir una fecha de inicio.");

            if (!ModelState.IsValid)
            {
                var fechaParaMostrar =
                    vm.FechaInicio == default
                        ? DateTime.Now
                        : vm.FechaInicio;

                var vmConDatos =
                    await ArmarViewModelAsync(
                        seleccion,
                        fechaParaMostrar);

                vmConDatos.Cliente = vm.Cliente;

                return View(vmConDatos);
            }

            var idsModelos = seleccion
                .Select(s => s.IdModelo)
                .ToList();

            var modelos = await _context.Modelo
                .Where(m => idsModelos.Contains(m.IdModelo))
                .ToListAsync();

            decimal horasTotales = 0;
            var detalles = new List<PedidoDetalle>();

            foreach (var item in seleccion)
            {
                var modelo = modelos
                    .FirstOrDefault(m => m.IdModelo == item.IdModelo);

                if (modelo == null)
                    continue;

                horasTotales +=
                    modelo.TiempoProduccion * item.Cantidad;

                modelo.VecesAgregado += item.Cantidad;

                detalles.Add(new PedidoDetalle
                {
                    IdModelo = item.IdModelo,
                    Cantidad = item.Cantidad
                });
            }

            var (fechaInicioReal, fechaEntrega) =
                _colaProduccion.CalcularFechasPedido(
                    vm.FechaInicio,
                    horasTotales);

            var pedido = new Pedido
            {
                Cliente = vm.Cliente,
                FechaPedido = DateTime.Now,
                FechaInicio = fechaInicioReal,
                FechaEntrega = fechaEntrega,
                Estado = EstadoPedido.Pendiente,
                Detalles = detalles
            };

            _context.Pedido.Add(pedido);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove(SESSION_KEY_SELECCION);

            TempData["Mensaje"] =
                $"Pedido registrado. Fecha de entrega estimada: " +
                $"{fechaEntrega:dddd dd/MM/yyyy HH:mm}";

            return RedirectToAction(nameof(Index));
        }

        // Muestra el formulario para editar un pedido.
        public async Task<IActionResult> Edit(int id)
        {
            var pedido = await _context.Pedido
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            var vm = await ArmarEditViewModelAsync(pedido);

            return View(vm);
        }

        // Actualiza un pedido existente.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            PedidoEditViewModel vm)
        {
            if (id != vm.IdPedido)
                return NotFound();

            var pedidoSeleccionado = vm.ModelosDisponibles
                .Where(m => m.Cantidad > 0)
                .ToList();

            if (string.IsNullOrWhiteSpace(vm.Cliente))
                ModelState.AddModelError(
                    nameof(vm.Cliente),
                    "El cliente es obligatorio.");

            if (!pedidoSeleccionado.Any())
                ModelState.AddModelError(
                    "",
                    "Debe seleccionar al menos un modelo.");

            if (!ModelState.IsValid)
            {
                var pedidoOriginal = await _context.Pedido
                    .Include(p => p.Detalles)
                    .FirstOrDefaultAsync(p => p.IdPedido == id);

                if (pedidoOriginal == null)
                    return NotFound();

                var vmRecargado =
                    await ArmarEditViewModelAsync(pedidoOriginal);

                vmRecargado.Cliente = vm.Cliente;
                vmRecargado.FechaInicio = vm.FechaInicio;
                vmRecargado.Estado = vm.Estado;

                return View(vmRecargado);
            }

            var pedido = await _context.Pedido
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            var idsModelos = pedidoSeleccionado
                .Select(m => m.IdModelo)
                .ToList();

            var modelosReales = await _context.Modelo
                .Where(m => idsModelos.Contains(m.IdModelo))
                .ToListAsync();

            decimal horasTotales = 0;
            var nuevosDetalles = new List<PedidoDetalle>();

            foreach (var item in pedidoSeleccionado)
            {
                var modelo = modelosReales
                    .FirstOrDefault(m => m.IdModelo == item.IdModelo);

                if (modelo == null)
                    continue;

                horasTotales +=
                    modelo.TiempoProduccion * item.Cantidad;

                nuevosDetalles.Add(new PedidoDetalle
                {
                    IdPedido = pedido.IdPedido,
                    IdModelo = item.IdModelo,
                    Cantidad = item.Cantidad
                });
            }

            _context.PedidoDetalle.RemoveRange(pedido.Detalles);

            var (fechaInicioReal, fechaEntrega) =
                _colaProduccion.CalcularFechasPedido(
                    vm.FechaInicio,
                    horasTotales);

            pedido.Cliente = vm.Cliente;
            pedido.FechaInicio = fechaInicioReal;
            pedido.FechaEntrega = fechaEntrega;
            pedido.Estado = vm.Estado;
            pedido.Detalles = nuevosDetalles;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                $"Pedido actualizado. Nueva fecha de entrega: " +
                $"{fechaEntrega:dddd dd/MM/yyyy HH:mm}";

            return RedirectToAction(nameof(Index));
        }

        // Muestra la confirmación para eliminar un pedido.
        public async Task<IActionResult> Delete(int id)
        {
            var pedido = await _context.Pedido
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Modelo)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            return View(pedido);
        }

        // Elimina un pedido.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmado(int id)
        {
            var pedido = await _context.Pedido.FindAsync(id);

            if (pedido != null)
            {
                _context.Pedido.Remove(pedido);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Pedido eliminado.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Calcula el tiempo restante para la entrega.
        public string CalcularTiempoRestante(DateTime fechaEntrega)
        {
            var restante = fechaEntrega - DateTime.Now;

            if (restante.TotalHours <= 0)
                return "Vencido";

            int dias = restante.Days;
            int horas = restante.Hours;

            return dias == 0
                ? $"{horas} horas"
                : $"{dias} días, {horas} horas";
        }

        private List<SeleccionModeloDto>? ObtenerSeleccionDeSesion()
        {
            var json = HttpContext.Session
                .GetString(SESSION_KEY_SELECCION);

            return string.IsNullOrEmpty(json)
                ? null
                : JsonSerializer.Deserialize<List<SeleccionModeloDto>>(json);
        }

        private async Task<PedidoCreateViewModel> ArmarViewModelAsync(
            List<SeleccionModeloDto> seleccion,
            DateTime fechaInicioPropuesta)
        {
            var idsModelos = seleccion
                .Select(s => s.IdModelo)
                .ToList();

            var modelos = await _context.Modelo
                .Where(m => idsModelos.Contains(m.IdModelo))
                .ToListAsync();

            var detalles = seleccion
                .Where(s => modelos.Any(m => m.IdModelo == s.IdModelo))
                .Select(s =>
                {
                    var modelo = modelos
                        .First(m => m.IdModelo == s.IdModelo);

                    return new PedidoDetalleViewModel
                    {
                        IdModelo = modelo.IdModelo,
                        Nombre = modelo.Nombre,
                        Imagen = modelo.Imagen,
                        Cantidad = s.Cantidad,
                        TiempoProduccion = modelo.TiempoProduccion,
                        PrecioVenta = modelo.PrecioVenta
                    };
                })
                .ToList();

            var horasTotales = detalles
                .Sum(d => d.TiempoProduccion * d.Cantidad);

            var fechaInicioSugerida =
                _colaProduccion.ObtenerFechaFinCola();

            return new PedidoCreateViewModel
            {
                Detalles = detalles,
                HorasProduccionTotal = horasTotales,
                PrecioVentaTotal = detalles.Sum(d => d.Subtotal),
                FechaInicioSugerida = fechaInicioSugerida,
                FechaInicio = fechaInicioPropuesta
            };
        }

        private async Task<PedidoEditViewModel> ArmarEditViewModelAsync(
            Pedido pedido)
        {
            var catalogoCompleto =
                await _context.Modelo.ToListAsync();

            var modelosDisponibles = catalogoCompleto
                .Select(modelo =>
                {
                    var detalleExistente = pedido.Detalles
                        .FirstOrDefault(
                            d => d.IdModelo == modelo.IdModelo);

                    return new ModeloSeleccionViewModel
                    {
                        IdModelo = modelo.IdModelo,
                        Nombre = modelo.Nombre,
                        Imagen = modelo.Imagen,
                        TiempoProduccion = modelo.TiempoProduccion,
                        PrecioVenta = modelo.PrecioVenta,
                        Cantidad = detalleExistente?.Cantidad ?? 0
                    };
                })
                .ToList();

            return new PedidoEditViewModel
            {
                IdPedido = pedido.IdPedido,
                Cliente = pedido.Cliente,
                FechaInicio = pedido.FechaInicio,
                Estado = pedido.Estado,
                ModelosDisponibles = modelosDisponibles
            };
        }
    }
}