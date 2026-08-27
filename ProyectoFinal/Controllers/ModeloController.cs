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

        // Bs. por hora de mano de obra.
        private const decimal PRECIO_POR_HORA = 5m;

        public ModeloController(TiendaDbContext context)
        {
            _context = context;
        }

        // GET: Modelo
        public async Task<IActionResult> Index()
        {
            var modelos = await _context.Modelo
                .Include(m => m.ModeloMateriales)
                    .ThenInclude(mm => mm.Material)
                .ToListAsync();
            return View(modelos);
        }

        // GET: Modelo/Catalogo
        // .Include(...) le pide a Entity Framework que también traiga, para cada modelo,
        // su lista de ModeloMateriales. .ThenInclude(...) va un paso más allá y trae,
        // para cada ModeloMaterial, los datos del Material relacionado (nombre, etc.).
        // Sin estas dos líneas, modelo.ModeloMateriales siempre llega vacío a la vista.
        public async Task<IActionResult> Catalogo()
        {
            var modelos = await _context.Modelo
                .Include(m => m.ModeloMateriales)
                    .ThenInclude(mm => mm.Material)
                .ToListAsync();
            return View(modelos);
        }

        // GET: Modelo/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Materiales = await _context.Material.ToListAsync();
            return View();
        }

        // POST: Modelo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Modelo modelo, List<int> materialesSeleccionados, List<decimal> cantidades)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Materiales = await _context.Material.ToListAsync();
                return View(modelo);
            }

            var costoMateriales = await CalcularCostoAsync(materialesSeleccionados, cantidades);
            var costoManoObra = modelo.TiempoProduccion * PRECIO_POR_HORA;
            modelo.Costo = costoMateriales + costoManoObra;
            modelo.PrecioVenta = CalcularPrecioVenta(modelo.Costo, modelo.Dificultad);

            _context.Add(modelo);
            await _context.SaveChangesAsync(); // Necesario primero para tener modelo.IdModelo generado.

            AgregarModeloMateriales(modelo.IdModelo, materialesSeleccionados, cantidades);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Modelo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var modelo = await _context.Modelo.FindAsync(id);
            if (modelo == null) return NotFound();

            ViewBag.Materiales = await _context.Material.ToListAsync();

            // Materiales ya asociados a este modelo, para precargarlos en la vista.
            ViewBag.MaterialesSeleccionadosActuales = await _context.ModeloMaterial
                .Where(mm => mm.IdModelo == id)
                .ToListAsync();

            return View(modelo);
        }

        // POST: Modelo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Modelo modelo, List<int> materialesSeleccionados, List<decimal> cantidades)
        {
            if (id != modelo.IdModelo) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Materiales = await _context.Material.ToListAsync();
                return View(modelo);
            }

            var costoMateriales = await CalcularCostoAsync(materialesSeleccionados, cantidades);
            var costoManoObra = modelo.TiempoProduccion * PRECIO_POR_HORA;
            modelo.Costo = costoMateriales + costoManoObra;
            modelo.PrecioVenta = CalcularPrecioVenta(modelo.Costo, modelo.Dificultad);

            try
            {
                _context.Update(modelo);

                // Reemplaza las asociaciones anteriores por las nuevas seleccionadas.
                var anteriores = _context.ModeloMaterial.Where(mm => mm.IdModelo == id);
                _context.ModeloMaterial.RemoveRange(anteriores);

                AgregarModeloMateriales(id, materialesSeleccionados, cantidades);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ModeloExists(modelo.IdModelo)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Modelo/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var modelo = await _context.Modelo.FirstOrDefaultAsync(m => m.IdModelo == id);
            if (modelo == null) return NotFound();

            return View(modelo);
        }

        // POST: Modelo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var modelo = await _context.Modelo.FindAsync(id);
            if (modelo != null)
            {
                // Elimina primero las asociaciones para no violar la llave foránea.
                var relaciones = _context.ModeloMaterial.Where(mm => mm.IdModelo == id);
                _context.ModeloMaterial.RemoveRange(relaciones);

                _context.Modelo.Remove(modelo);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ModeloExists(int id)
        {
            return _context.Modelo.Any(e => e.IdModelo == id);
        }

        // Suma (precio_unitario x cantidad) de cada material elegido.
        private async Task<decimal> CalcularCostoAsync(List<int> materialesSeleccionados, List<decimal> cantidades)
        {
            if (materialesSeleccionados == null || cantidades == null)
                return 0;

            decimal costo = 0;
            for (int i = 0; i < materialesSeleccionados.Count; i++)
            {
                var material = await _context.Material.FindAsync(materialesSeleccionados[i]);
                if (material != null)
                    costo += material.PrecioUnitario * cantidades[i];
            }
            return costo;
        }

        // Calcula el precio de venta a partir del costo total (materiales + mano de obra)
        // aplicando un multiplicador de ganancia según el nivel de dificultad de la obra.
        private decimal CalcularPrecioVenta(decimal costo, string dificultad)
        {
            decimal multiplicador = dificultad?.Trim().ToLower() switch
            {
                "baja" => 1.3m,
                "media" => 1.6m,
                "alta" => 2.0m,
                _ => Modelo.PORCENTAJE_GANANCIA // respaldo si el valor no coincide con ninguno
            };

            return costo * multiplicador;
        }

        // Crea las filas de ModeloMaterial en memoria (se guardan con el SaveChangesAsync siguiente).
        // Si el usuario elige el mismo material en más de una fila del formulario, se suman
        // las cantidades en vez de crear dos registros con la misma llave (IdModelo + IdMaterial),
        // lo que evita el error "cannot be tracked because another instance with the same key...".
        private void AgregarModeloMateriales(int idModelo, List<int> materialesSeleccionados, List<decimal> cantidades)
        {
            if (materialesSeleccionados == null || cantidades == null)
                return;

            var cantidadPorMaterial = new Dictionary<int, decimal>();

            for (int i = 0; i < materialesSeleccionados.Count; i++)
            {
                int idMaterial = materialesSeleccionados[i];
                decimal cantidad = cantidades[i];

                if (cantidadPorMaterial.ContainsKey(idMaterial))
                    cantidadPorMaterial[idMaterial] += cantidad;
                else
                    cantidadPorMaterial[idMaterial] = cantidad;
            }

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