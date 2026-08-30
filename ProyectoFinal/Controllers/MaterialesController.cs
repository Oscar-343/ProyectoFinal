using Microsoft.AspNetCore.Mvc;
using ProyectoFinal.Data;
using ProyectoFinal.Models;
using ProyectoFinal.Filters;

namespace ProyectoFinal.Controllers
{
    [RequiereSesion]
    // Controlador encargado de gestionar los materiales.
    public class MaterialesController : Controller
    {
        private readonly TiendaDbContext _context;

        public MaterialesController(TiendaDbContext context)
        {
            _context = context;
        }

        //Estado del material comparando su cantidad disponible contra el stock mínimo.
        private string CalcularEstado(material material)
        {
            if (material.CantidadDisponible <= 0)
            {
                return "stock agotado";
            }
            else if (material.CantidadDisponible <= material.StockMinimo)
            {
                return "stock bajo";
            }
            else
            {
                return "stock disponible";
            }
        }

        // Lista todos los materiales registrados.
        public IActionResult Index()
        {
            var materiales = _context.Material.ToList();
            return View(materiales);
        }

        // Muestra el detalle de un material específico.
        public IActionResult Details(int id)
        {
            var material = _context.Material.Find(id);
            if (material == null)
            {
                return NotFound();
            }
            return View(material);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
            public IActionResult Create(material material)
            {
           
                ModelState.Remove("Estado");

                if (ModelState.IsValid)
                {
                    material.Estado = CalcularEstado(material);

                    _context.Material.Add(material);
                    _context.SaveChanges();

                    return RedirectToAction("Index");
                }

                return View(material);
            }

        // Muestra el formulario de edición con los datos actuales del material.
        public IActionResult Edit(int id)
        {
            var material = _context.Material.Find(id);
            if (material == null)
            {
                return NotFound();
            }
            return View(material);
        }

        [HttpPost]
        public IActionResult Edit(material material)
        {
            ModelState.Remove("Estado");

            if (ModelState.IsValid)
            {
                material.Estado = CalcularEstado(material);

                _context.Material.Update(material);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(material);
        }

        // Muestra la pantalla de confirmación antes de borrar.
        public IActionResult Delete(int id)
        {
            var material = _context.Material.Find(id);
            if (material == null)
            {
                return NotFound();
            }
            return View(material);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            var material = _context.Material.Find(id);
            if (material != null)
            {
                _context.Material.Remove(material);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}