using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;

namespace ProyectoFinal.Controllers
{
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
        public async Task<IActionResult> Catalogo()
        {
            var modelos = await _context.Modelo.ToListAsync();
            return View(modelos);
        }

        // GET: Modelo/Create
        public IActionResult Create()
        {
            // TODO: cuando exista la tabla Material, cargar la lista para el <select>:
            // ViewBag.Materiales = await _context.Material.ToListAsync();
            return View();
        }

        // POST: Modelo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Modelo modelo /*, List<int> materialesSeleccionados, List<decimal> cantidades */)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            modelo.Costo = await CalcularCostoAsync(/* materialesSeleccionados, cantidades */);
            modelo.PrecioVenta = modelo.Costo * Modelo.PORCENTAJE_GANANCIA;

            _context.Add(modelo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Modelo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var modelo = await _context.Modelo.FindAsync(id);
            if (modelo == null) return NotFound();

            // TODO: cuando exista Material, cargar también la lista y los materiales
            // ya seleccionados para este modelo (desde ModeloMateriales).
            // ViewBag.Materiales = await _context.Material.ToListAsync();

            return View(modelo);
        }

        // POST: Modelo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Modelo modelo /*, List<int> materialesSeleccionados, List<decimal> cantidades */)
        {
            if (id != modelo.IdModelo) return NotFound();
            if (!ModelState.IsValid) return View(modelo);

            modelo.Costo = await CalcularCostoAsync(/* materialesSeleccionados, cantidades */);
            modelo.PrecioVenta = modelo.Costo * Modelo.PORCENTAJE_GANANCIA;

            try
            {
                _context.Update(modelo);
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
                _context.Modelo.Remove(modelo);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ModeloExists(int id)
        {
            return _context.Modelo.Any(e => e.IdModelo == id);
        }

        // Calcula el costo sumando (precio_unitario x cantidad) de cada material elegido.
        // Por ahora devuelve 0 porque la tabla Material todavía no existe.
        //
        // TODO: reemplazar el cuerpo de este método cuando exista Material, así:
        //
        // private async Task<decimal> CalcularCostoAsync(List<int> materialesSeleccionados, List<decimal> cantidades)
        // {
        //     decimal costo = 0;
        //     for (int i = 0; i < materialesSeleccionados.Count; i++)
        //     {
        //         var material = await _context.Material.FindAsync(materialesSeleccionados[i]);
        //         costo += material.PrecioUnitario * cantidades[i];
        //     }
        //     return costo;
        // }
        private Task<decimal> CalcularCostoAsync()
        {
            return Task.FromResult(0m);
        }
    }
}