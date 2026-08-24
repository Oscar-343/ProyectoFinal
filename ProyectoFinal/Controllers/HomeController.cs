using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;
using System.Diagnostics;
using ProyectoFinal.Filters;

namespace ProyectoFinal.Controllers
{
    [RequiereSesion]
    public class HomeController : Controller
    {
        private readonly TiendaDbContext _context;

        public HomeController(TiendaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Si tu compañero guarda el nombre en sesión con otra clave, ajusta esto.
            ViewBag.NombreUsuario = HttpContext.Session.GetString("UsuarioLogueado") ?? "Administradora";

            var dashboard = new HomeDashboardViewModel
            {
                TotalModelos = await _context.Modelo.CountAsync(),
                TotalMateriales = await _context.Material.CountAsync(),

                // Ajusta CantidadDisponible / StockMinimo si tu compañero usó otros nombres de propiedad.
                MaterialesPorAcabarse = await _context.Material
                    .Where(m => m.CantidadDisponible <= m.StockMinimo)
                    .OrderBy(m => m.CantidadDisponible)
                    .ToListAsync(),

                // TODO: reemplazar cuando exista la tabla de ventas/pedidos (sprint futuro).
                // Por ahora queda vacío a propósito, la vista muestra un estado "próximamente".
                ModelosMasVendidos = new List<Modelo>()
            };

            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}