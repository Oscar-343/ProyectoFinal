using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;
using System.Diagnostics;
using ProyectoFinal.Filters;

namespace ProyectoFinal.Controllers
{
    //Obliga a que el usuario haya iniciado sesión
    [RequiereSesion]
    public class HomeController : Controller
    {
        private readonly TiendaDbContext _context;

        // Se inyecta el DbContext para poder consultar la base de datos.
        public HomeController(TiendaDbContext context)
        {
            _context = context;
        }

        // Página principal (dashboard)
        public async Task<IActionResult> Index()
        {
            ViewBag.NombreUsuario = HttpContext.Session.GetString("UsuarioLogueado") ?? "Administradora";

            var dashboard = new HomeDashboardViewModel
            {
                // Cuenta cuántos modelos hay registrados en total.
                TotalModelos = await _context.Modelo.CountAsync(),

                // Cuenta cuántos materiales hay registrados en total.
                TotalMateriales = await _context.Material.CountAsync(),

                //Materiales cuyo stock ya llegó o bajó del mínimo
                MaterialesPorAcabarse = await _context.Material
                    .Where(m => m.CantidadDisponible <= m.StockMinimo)
                    .OrderBy(m => m.CantidadDisponible)
                    .ToListAsync(),

                ModelosMasVendidos = new List<Modelo>()
            };

            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Evita que el navegador guarde en caché la página de error.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}