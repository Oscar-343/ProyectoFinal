using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;
using ProyectoFinal.Filters;

namespace ProyectoFinal.Controllers
{
    [RequiereSesion]
    public class ModeloController : Controller
    {
        private readonly TiendaDbContext _context;

        // Costo de mano de obra por hora.
        private const decimal PRECIO_POR_HORA = 5m;

        public ModeloController(TiendaDbContext context)
        {
            _context = context;
        }

        // Muestra todos los modelos registrados.
        public async Task<IActionResult> Index()
        {
            var modelos = await _context.Modelo
                .Include(m => m.ModeloMateriales)
                    .ThenInclude(mm => mm.Material)
                .ToListAsync();

            return View(modelos);
        }

        // Muestra el catálogo de modelos.
        public async Task<IActionResult> Catalogo()
        {
            var modelos = await _context.Modelo
                .Include(m => m.ModeloMateriales)
                    .ThenInclude(mm => mm.Material)
                .ToListAsync();

            return View(modelos);
        }

        // Muestra el formulario para registrar un modelo.
        public async Task<IActionResult> Create()
        {
            ViewBag.Materiales = await _context.Material.ToListAsync();
            return View();
        }

        // Registra un nuevo modelo.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Modelo modelo,
            List<int> materialesSeleccionados,
            List<decimal> cantidades)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Materiales = await _context.Material.ToListAsync();
                return View(modelo);
            }

            // Calcula el costo total del modelo.
            var costoMateriales = await CalcularCostoAsync(
                materialesSeleccionados, cantidades);

            var costoManoObra = modelo.TiempoProduccion * PRECIO_POR_HORA;

            modelo.Costo = costoMateriales + costoManoObra;
            modelo.PrecioVenta = CalcularPrecioVenta(
                modelo.Costo, modelo.Dificultad);

            _context.Add(modelo);
            await _context.SaveChangesAsync();

            // Guarda los materiales utilizados por el modelo.
            AgregarModeloMateriales(
                modelo.IdModelo,
                materialesSeleccionados,
                cantidades);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Muestra el formulario para editar un modelo.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var modelo = await _context.Modelo.FindAsync(id);

            if (modelo == null) return NotFound();

            ViewBag.Materiales = await _context.Material.ToListAsync();

            // Obtiene los materiales asociados al modelo.
            ViewBag.MaterialesSeleccionadosActuales =
                await _context.ModeloMaterial
                    .Where(mm => mm.IdModelo == id)
                    .ToListAsync();

            return View(modelo);
        }

        // Actualiza un modelo existente.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Modelo modelo,
            List<int> materialesSeleccionados,
            List<decimal> cantidades)
        {
            if (id != modelo.IdModelo) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Materiales = await _context.Material.ToListAsync();
                return View(modelo);
            }

            // Recalcula los costos del modelo.
            var costoMateriales = await CalcularCostoAsync(
                materialesSeleccionados, cantidades);

            var costoManoObra = modelo.TiempoProduccion * PRECIO_POR_HORA;

            modelo.Costo = costoMateriales + costoManoObra;
            modelo.PrecioVenta = CalcularPrecioVenta(
                modelo.Costo, modelo.Dificultad);

            try
            {
                _context.Update(modelo);

                // Reemplaza los materiales anteriores por los nuevos.
                var anteriores = _context.ModeloMaterial
                    .Where(mm => mm.IdModelo == id);

                _context.ModeloMaterial.RemoveRange(anteriores);

                AgregarModeloMateriales(
                    id,
                    materialesSeleccionados,
                    cantidades);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ModeloExists(modelo.IdModelo))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // Muestra la confirmación para eliminar un modelo.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var modelo = await _context.Modelo
                .FirstOrDefaultAsync(m => m.IdModelo == id);

            if (modelo == null) return NotFound();

            return View(modelo);
        }

        // Elimina un modelo y sus materiales asociados.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var modelo = await _context.Modelo.FindAsync(id);

            if (modelo != null)
            {
                var relaciones = _context.ModeloMaterial
                    .Where(mm => mm.IdModelo == id);

                _context.ModeloMaterial.RemoveRange(relaciones);
                _context.Modelo.Remove(modelo);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Comprueba si existe un modelo.
        private bool ModeloExists(int id)
        {
            return _context.Modelo.Any(e => e.IdModelo == id);
        }

        // Calcula el costo de los materiales utilizados.
        private async Task<decimal> CalcularCostoAsync(
            List<int> materialesSeleccionados,
            List<decimal> cantidades)
        {
            if (materialesSeleccionados == null || cantidades == null)
                return 0;

            decimal costo = 0;

            for (int i = 0; i < materialesSeleccionados.Count; i++)
            {
                var material = await _context.Material
                    .FindAsync(materialesSeleccionados[i]);

                if (material != null)
                    costo += material.PrecioUnitario * cantidades[i];
            }

            return costo;
        }

        // Calcula el precio de venta según la dificultad.
        private decimal CalcularPrecioVenta(
            decimal costo,
            string dificultad)
        {
            decimal multiplicador = dificultad?.Trim().ToLower() switch
            {
                "baja" => 1.3m,
                "media" => 1.6m,
                "alta" => 2.0m,
                _ => Modelo.PORCENTAJE_GANANCIA
            };

            return costo * multiplicador;
        }

        // Asocia los materiales seleccionados al modelo.
        private void AgregarModeloMateriales(
            int idModelo,
            List<int> materialesSeleccionados,
            List<decimal> cantidades)
        {
            if (materialesSeleccionados == null || cantidades == null)
                return;

            var cantidadPorMaterial = new Dictionary<int, decimal>();

            // Agrupa las cantidades cuando se repite un material.
            for (int i = 0; i < materialesSeleccionados.Count; i++)
            {
                int idMaterial = materialesSeleccionados[i];
                decimal cantidad = cantidades[i];

                if (cantidadPorMaterial.ContainsKey(idMaterial))
                    cantidadPorMaterial[idMaterial] += cantidad;
                else
                    cantidadPorMaterial[idMaterial] = cantidad;
            }

            // Crea las relaciones entre modelo y material.
            foreach (var par in cantidadPorMaterial)
            {
                _context.ModeloMaterial.Add(new ModeloMaterial
                {
                    IdModelo = idModelo,
                    IdMaterial = par.Key,
                    Cantidad = par.Value
                });
            }
        }
    }
}