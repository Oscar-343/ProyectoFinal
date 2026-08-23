using Microsoft.AspNetCore.Mvc;
using ProyectoFinal.Data;
using ProyectoFinal.Models;

namespace ProyectoFinal.Controllers
{
    // Controlador encargado de gestionar los materiales.
    public class MaterialesController : Controller
    {
        private readonly TiendaDbContext _context;

        public MaterialesController(TiendaDbContext context)
        {
            _context = context;
        }

        // Calcula el estado del material según su stock.
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

        // =====================================================
        // MOSTRAR TODOS LOS MATERIALES
        // =====================================================
        public IActionResult Index()
        {
            var materiales = _context.Material.ToList();
            return View(materiales);
        }

        // =====================================================
        // MOSTRAR UN MATERIAL
        // =====================================================
        public IActionResult Details(int id)
        {
            var material = _context.Material.Find(id);
            if (material == null)
            {
                return NotFound();
            }
            return View(material);
        }

        // =====================================================
        // MOSTRAR FORMULARIO PARA CREAR
        // =====================================================
        public IActionResult Create()
        {
            return View();
        }

        // =====================================================
        // GUARDAR NUEVO MATERIAL
        // =====================================================
        [HttpPost]
        public IActionResult Create(material material)
        {
            // El estado se calcula automáticamente,
            // no lo llena el usuario en el formulario.
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

        // =====================================================
        // MOSTRAR FORMULARIO PARA EDITAR
        // =====================================================
        public IActionResult Edit(int id)
        {
            var material = _context.Material.Find(id);
            if (material == null)
            {
                return NotFound();
            }
            return View(material);
        }

        // =====================================================
        // GUARDAR CAMBIOS
        // =====================================================
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

        // =====================================================
        // MOSTRAR CONFIRMACIÓN DE ELIMINACIÓN
        // =====================================================
        public IActionResult Delete(int id)
        {
            var material = _context.Material.Find(id);
            if (material == null)
            {
                return NotFound();
            }
            return View(material);
        }

        // =====================================================
        // ELIMINAR MATERIAL
        // =====================================================
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