using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Filters;
using ProyectoFinal.Models;
using ProyectoFinal.Services;
using ProyectoFinal.ViewModels;

namespace ProyectoFinal.Controllers
{
    [RequiereSesion]
    public class PedidoPersonalizadoController : Controller
    {
        private readonly TiendaDbContext _context;
        private readonly IColaProduccionService _colaProduccion;
        private readonly IWebHostEnvironment _entorno;

        private const decimal PRECIO_POR_HORA = 5m;

        private static readonly string[] EXTENSIONES_PERMITIDAS = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long TAMANO_MAXIMO_IMAGEN = 5 * 1024 * 1024; // 5 MB

        public PedidoPersonalizadoController(
            TiendaDbContext context,
            IColaProduccionService colaProduccion,
            IWebHostEnvironment entorno)
        {
            _context = context;
            _colaProduccion = colaProduccion;
            _entorno = entorno;
        }

        // GET: PedidoPersonalizado
        public async Task<IActionResult> Index()
        {
            var pedidos = await _context.PedidoPersonalizado
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            return View(pedidos);
        }

        // GET: PedidoPersonalizado/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var pedido = await _context.PedidoPersonalizado
                .Include(p => p.Materiales)
                    .ThenInclude(m => m.Material)
                .FirstOrDefaultAsync(p => p.IdPedidoPersonalizado == id);

            if (pedido == null)
                return NotFound();

            return View(pedido);
        }

        // GET: PedidoPersonalizado/Create
        public async Task<IActionResult> Create()
        {
            var vm = await ArmarViewModelAsync(DateTime.Now);
            ViewBag.Materiales = await _context.Material.OrderBy(m => m.Nombre).ToListAsync();
            return View(vm);
        }

        // POST: PedidoPersonalizado/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
        PedidoPersonalizadoCreateViewModel vm,
        List<int> materialesSeleccionados,
        List<decimal> cantidades)
        {
            var seleccion = new List<(int IdMaterial, decimal Cantidad)>();
            if (materialesSeleccionados != null && cantidades != null)
            {
                for (int i = 0; i < materialesSeleccionados.Count && i < cantidades.Count; i++)
                {
                    if (cantidades[i] > 0)
                        seleccion.Add((materialesSeleccionados[i], cantidades[i]));
                }
            }

            if (string.IsNullOrWhiteSpace(vm.Cliente))
                ModelState.AddModelError(nameof(vm.Cliente), "El cliente es obligatorio.");

            if (string.IsNullOrWhiteSpace(vm.NombreReferencia))
                ModelState.AddModelError(nameof(vm.NombreReferencia), "Da un nombre de referencia.");

            if (vm.TiempoProduccion <= 0)
                ModelState.AddModelError(nameof(vm.TiempoProduccion), "El tiempo de producción debe ser mayor a 0.");

            if (!seleccion.Any())
                ModelState.AddModelError("", "Selecciona al menos un material y su cantidad.");

            if (vm.ImagenArchivo != null && vm.ImagenArchivo.Length > 0)
            {
                var extension = Path.GetExtension(vm.ImagenArchivo.FileName).ToLowerInvariant();
                if (!EXTENSIONES_PERMITIDAS.Contains(extension))
                    ModelState.AddModelError("", "La imagen debe ser jpg, png o webp.");
                else if (vm.ImagenArchivo.Length > TAMANO_MAXIMO_IMAGEN)
                    ModelState.AddModelError("", "La imagen no puede pesar más de 5MB.");
            }

            if (!ModelState.IsValid)
            {
                var fechaParaMostrar = vm.FechaInicio == default ? DateTime.Now : vm.FechaInicio;
                var vmRecargado = await ArmarViewModelAsync(fechaParaMostrar);

                vmRecargado.Cliente = vm.Cliente;
                vmRecargado.NombreReferencia = vm.NombreReferencia;
                vmRecargado.Descripcion = vm.Descripcion;
                vmRecargado.Dificultad = vm.Dificultad;
                vmRecargado.TiempoProduccion = vm.TiempoProduccion;

                // Conserva lo que el usuario ya había elegido, para que el JS lo prellene de nuevo.
                vmRecargado.MaterialesDisponibles = seleccion
                    .Select(s => new MaterialSeleccionViewModel { IdMaterial = s.IdMaterial, Cantidad = s.Cantidad })
                    .ToList();

                ViewBag.Materiales = await _context.Material.OrderBy(m => m.Nombre).ToListAsync();
                return View(vmRecargado);
            }

            var idsMateriales = seleccion.Select(s => s.IdMaterial).ToList();
            var materialesReales = await _context.Material
                .Where(m => idsMateriales.Contains(m.IdMaterial))
                .ToListAsync();

            decimal costoMateriales = 0;
            var detallesMaterial = new List<PedidoPersonalizadoMaterial>();

            foreach (var item in seleccion)
            {
                var material = materialesReales.FirstOrDefault(m => m.IdMaterial == item.IdMaterial);
                if (material == null) continue;

                costoMateriales += material.PrecioUnitario * item.Cantidad;

                detallesMaterial.Add(new PedidoPersonalizadoMaterial
                {
                    IdMaterial = item.IdMaterial,
                    Cantidad = item.Cantidad
                });
            }

            var costoManoObra = vm.TiempoProduccion * PRECIO_POR_HORA;
            var costoTotal = costoMateriales + costoManoObra;
            var precioVenta = costoTotal * ObtenerMultiplicador(vm.Dificultad);

            string? rutaImagen = null;
            if (vm.ImagenArchivo != null && vm.ImagenArchivo.Length > 0)
                rutaImagen = await GuardarImagenAsync(vm.ImagenArchivo);

            var (fechaInicioReal, fechaEntrega) = _colaProduccion.CalcularFechasPedido(vm.FechaInicio, vm.TiempoProduccion);

            var pedido = new PedidoPersonalizado
            {
                Cliente = vm.Cliente,
                NombreReferencia = vm.NombreReferencia,
                Descripcion = vm.Descripcion,
                Imagen = rutaImagen,
                Dificultad = vm.Dificultad,
                TiempoProduccion = vm.TiempoProduccion,
                Costo = costoTotal,
                PrecioVenta = precioVenta,
                FechaPedido = DateTime.Now,
                FechaInicio = fechaInicioReal,
                FechaEntrega = fechaEntrega,
                Estado = EstadoPedido.Pendiente,
                Materiales = detallesMaterial
            };

            _context.PedidoPersonalizado.Add(pedido);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                $"Pedido personalizado registrado. Precio de venta: Bs {precioVenta:0.00}. " +
                $"Entrega estimada: {fechaEntrega:dddd dd/MM/yyyy HH:mm}";

            return RedirectToAction(nameof(Index));
        }

        // GET: PedidoPersonalizado/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var pedido = await _context.PedidoPersonalizado
                .Include(p => p.Materiales)
                .FirstOrDefaultAsync(p => p.IdPedidoPersonalizado == id);

            if (pedido == null)
                return NotFound();

            var vm = await ArmarEditViewModelAsync(pedido);

            // Catálogo completo de materiales para el <select> de la plantilla de filas.
            ViewBag.Materiales = await _context.Material.OrderBy(m => m.Nombre).ToListAsync();

            return View(vm);
        }

        // POST: PedidoPersonalizado/Edit/5
        // Recibe los materiales como dos listas paralelas (mismo patrón que ModeloController):
        // materialesSeleccionados[i] es el id del material elegido en la fila i,
        // cantidades[i] es la cantidad escrita en esa misma fila.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            PedidoPersonalizadoEditViewModel vm,
            List<int> materialesSeleccionados,
            List<decimal> cantidades)
        {
            if (id != vm.IdPedidoPersonalizado)
                return NotFound();

            // Arma pares (idMaterial, cantidad) a partir de las dos listas paralelas.
            var seleccion = new List<(int IdMaterial, decimal Cantidad)>();
            if (materialesSeleccionados != null && cantidades != null)
            {
                for (int i = 0; i < materialesSeleccionados.Count && i < cantidades.Count; i++)
                {
                    if (cantidades[i] > 0)
                        seleccion.Add((materialesSeleccionados[i], cantidades[i]));
                }
            }

            if (string.IsNullOrWhiteSpace(vm.Cliente))
                ModelState.AddModelError(nameof(vm.Cliente), "El cliente es obligatorio.");

            if (string.IsNullOrWhiteSpace(vm.NombreReferencia))
                ModelState.AddModelError(nameof(vm.NombreReferencia), "Da un nombre de referencia.");

            if (vm.TiempoProduccion <= 0)
                ModelState.AddModelError(nameof(vm.TiempoProduccion), "El tiempo de producción debe ser mayor a 0.");

            if (!seleccion.Any())
                ModelState.AddModelError("", "Selecciona al menos un material y su cantidad.");

            if (vm.ImagenArchivo != null && vm.ImagenArchivo.Length > 0)
            {
                var extension = Path.GetExtension(vm.ImagenArchivo.FileName).ToLowerInvariant();
                if (!EXTENSIONES_PERMITIDAS.Contains(extension))
                    ModelState.AddModelError("", "La imagen debe ser jpg, png o webp.");
                else if (vm.ImagenArchivo.Length > TAMANO_MAXIMO_IMAGEN)
                    ModelState.AddModelError("", "La imagen no puede pesar más de 5MB.");
            }

            if (!ModelState.IsValid)
            {
                var pedidoOriginal = await _context.PedidoPersonalizado
                    .Include(p => p.Materiales)
                    .FirstOrDefaultAsync(p => p.IdPedidoPersonalizado == id);

                if (pedidoOriginal == null)
                    return NotFound();

                var vmRecargado = await ArmarEditViewModelAsync(pedidoOriginal);
                vmRecargado.Cliente = vm.Cliente;
                vmRecargado.NombreReferencia = vm.NombreReferencia;
                vmRecargado.Descripcion = vm.Descripcion;
                vmRecargado.Dificultad = vm.Dificultad;
                vmRecargado.TiempoProduccion = vm.TiempoProduccion;
                vmRecargado.FechaInicio = vm.FechaInicio;
                vmRecargado.Estado = vm.Estado;

                // Conserva lo que el usuario ya había elegido en el formulario,
                // en vez de lo que el pedido tenía guardado en BD.
                vmRecargado.MaterialesDisponibles = seleccion
                    .Select(s => new MaterialSeleccionViewModel { IdMaterial = s.IdMaterial, Cantidad = s.Cantidad })
                    .ToList();

                ViewBag.Materiales = await _context.Material.OrderBy(m => m.Nombre).ToListAsync();
                return View(vmRecargado);
            }

            var pedido = await _context.PedidoPersonalizado
                .Include(p => p.Materiales)
                .FirstOrDefaultAsync(p => p.IdPedidoPersonalizado == id);

            if (pedido == null)
                return NotFound();

            var idsMateriales = seleccion.Select(s => s.IdMaterial).ToList();
            var materialesReales = await _context.Material
                .Where(m => idsMateriales.Contains(m.IdMaterial))
                .ToListAsync();

            decimal costoMateriales = 0;
            var nuevosDetalles = new List<PedidoPersonalizadoMaterial>();

            foreach (var item in seleccion)
            {
                var material = materialesReales.FirstOrDefault(m => m.IdMaterial == item.IdMaterial);
                if (material == null) continue;

                costoMateriales += material.PrecioUnitario * item.Cantidad;

                nuevosDetalles.Add(new PedidoPersonalizadoMaterial
                {
                    IdPedidoPersonalizado = pedido.IdPedidoPersonalizado,
                    IdMaterial = item.IdMaterial,
                    Cantidad = item.Cantidad
                });
            }

            // Reemplaza los materiales anteriores por los nuevos.
            _context.PedidoPersonalizadoMaterial.RemoveRange(pedido.Materiales);

            var costoManoObra = vm.TiempoProduccion * PRECIO_POR_HORA;
            var costoTotal = costoMateriales + costoManoObra;
            var precioVenta = costoTotal * ObtenerMultiplicador(vm.Dificultad);

            // Si suben una imagen nueva, reemplaza a la anterior. Si no, se conserva la que ya tenía.
            string? rutaImagen = pedido.Imagen;
            if (vm.ImagenArchivo != null && vm.ImagenArchivo.Length > 0)
                rutaImagen = await GuardarImagenAsync(vm.ImagenArchivo);

            var (fechaInicioReal, fechaEntrega) = _colaProduccion.CalcularFechasPedido(vm.FechaInicio, vm.TiempoProduccion);

            pedido.Cliente = vm.Cliente;
            pedido.NombreReferencia = vm.NombreReferencia;
            pedido.Descripcion = vm.Descripcion;
            pedido.Imagen = rutaImagen;
            pedido.Dificultad = vm.Dificultad;
            pedido.TiempoProduccion = vm.TiempoProduccion;
            pedido.Costo = costoTotal;
            pedido.PrecioVenta = precioVenta;
            pedido.FechaInicio = fechaInicioReal;
            pedido.FechaEntrega = fechaEntrega;
            pedido.Estado = vm.Estado;
            pedido.Materiales = nuevosDetalles;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                $"Pedido personalizado actualizado. Nuevo precio de venta: Bs {precioVenta:0.00}. " +
                $"Entrega estimada: {fechaEntrega:dddd dd/MM/yyyy HH:mm}";

            return RedirectToAction(nameof(Index));
        }

        // GET: PedidoPersonalizado/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var pedido = await _context.PedidoPersonalizado
                .Include(p => p.Materiales)
                    .ThenInclude(m => m.Material)
                .FirstOrDefaultAsync(p => p.IdPedidoPersonalizado == id);

            if (pedido == null) return NotFound();

            return View(pedido);
        }

        // POST: PedidoPersonalizado/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmado(int id)
        {
            var pedido = await _context.PedidoPersonalizado.FindAsync(id);
            if (pedido != null)
            {
                _context.PedidoPersonalizado.Remove(pedido);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Pedido personalizado eliminado.";
            }

            return RedirectToAction(nameof(Index));
        }

        private decimal ObtenerMultiplicador(string dificultad)
        {
            return dificultad?.Trim().ToLower() switch
            {
                "baja" => 1.3m,
                "media" => 1.6m,
                "alta" => 2.0m,
                _ => Modelo.PORCENTAJE_GANANCIA
            };
        }

        private async Task<string> GuardarImagenAsync(IFormFile archivo)
        {
            var carpetaDestino = Path.Combine(_entorno.WebRootPath, "uploads", "personalizados");
            Directory.CreateDirectory(carpetaDestino);

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaFisica = Path.Combine(carpetaDestino, nombreArchivo);

            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return $"/uploads/personalizados/{nombreArchivo}";
        }

        private async Task<PedidoPersonalizadoCreateViewModel> ArmarViewModelAsync(DateTime fechaInicioPropuesta)
        {
            return new PedidoPersonalizadoCreateViewModel
            {
                FechaInicio = fechaInicioPropuesta,
                FechaInicioSugerida = _colaProduccion.ObtenerFechaFinCola(),
                MaterialesDisponibles = new List<MaterialSeleccionViewModel>()
            };
        }

        // Arma el ViewModel de edición: SOLO con los materiales que el pedido
        // ya tenía seleccionados (no todo el catálogo). La vista usa esta lista
        // para prellenar las filas dinámicas por JavaScript.
        private async Task<PedidoPersonalizadoEditViewModel> ArmarEditViewModelAsync(PedidoPersonalizado pedido)
        {
            var materialesSeleccionados = pedido.Materiales
                .Select(pm => new MaterialSeleccionViewModel
                {
                    IdMaterial = pm.IdMaterial,
                    Cantidad = pm.Cantidad
                })
                .ToList();

            return new PedidoPersonalizadoEditViewModel
            {
                IdPedidoPersonalizado = pedido.IdPedidoPersonalizado,
                Cliente = pedido.Cliente,
                NombreReferencia = pedido.NombreReferencia,
                Descripcion = pedido.Descripcion,
                ImagenActual = pedido.Imagen,
                Dificultad = pedido.Dificultad,
                TiempoProduccion = pedido.TiempoProduccion,
                FechaInicio = pedido.FechaInicio,
                Estado = pedido.Estado,
                MaterialesDisponibles = materialesSeleccionados
            };
        }
    }
}