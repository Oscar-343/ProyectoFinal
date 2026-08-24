using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;
using ProyectoFinal.Filters;

namespace ProyectoFinal.Controllers
{
    // Requiere sesión activa
    [RequiereSesion]
    public class ModeloController : Controller
    {
        private readonly TiendaDbContext _context;

        public ModeloController(TiendaDbContext context)
        {
            _context = context;
        }

        // GET: Modelo
        public async Task<IActionResult> Index()
        {
            var modelos = await _context.Modelo.ToListAsync();
            return View(modelos);
        }

        // GET: Modelo/Catalogo
        // Trae los materiales de cada modelo (Include/ThenInclude)
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

            // Costo y precio de venta
            modelo.Costo = await CalcularCostoAsync(materialesSeleccionados, cantidades);
            modelo.PrecioVenta = modelo.Costo * Modelo.PORCENTAJE_GANANCIA;

            _context.Add(modelo);
            await _context.SaveChangesAsync(); // Se guarda primero para que el modelo tenga su Id.

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

            // Se vuelven a calcular por si cambiaron los materiales o cantidades.
            modelo.Costo = await CalcularCostoAsync(materialesSeleccionados, cantidades);
            modelo.PrecioVenta = modelo.Costo * Modelo.PORCENTAJE_GANANCIA;

            try
            {
                _context.Update(modelo);

                // Reemplaza las asociaciones anteriores por las nuevas seleccionadas
                var anteriores = _context.ModeloMaterial.Where(mm => mm.IdModelo == id);
                _context.ModeloMaterial.RemoveRange(anteriores);

                AgregarModeloMateriales(id, materialesSeleccionados, cantidades);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Pasa si otra persona/proceso modificó o borró el mismo modelo al mismo tiempo.
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
        // ActionName("Delete") hace que este método responda al POST de la vista "Delete",
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

        // Verifica si un modelo con ese Id todavía existe (usado en caso de conflicto al editar).
        private bool ModeloExists(int id)
        {
            return _context.Modelo.Any(e => e.IdModelo == id);
        }

        // Suma (precio_unitario x cantidad) de cada material elegido, para obtener el costo total del modelo.
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

        // Crea las filas de ModeloMaterial en memoria
        private void AgregarModeloMateriales(int idModelo, List<int> materialesSeleccionados, List<decimal> cantidades)
        {
            if (materialesSeleccionados == null || cantidades == null)
                return;

            for (int i = 0; i < materialesSeleccionados.Count; i++)
            {
                _context.ModeloMaterial.Add(new ModeloMaterial
                {
                    IdModelo = idModelo,
                    IdMaterial = materialesSeleccionados[i],
                    Cantidad = cantidades[i]
                });
            }
        }
    }
}