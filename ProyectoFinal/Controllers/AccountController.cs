using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;
using System.Linq;

namespace ProyectoFinal.Controllers
{
    public class AccountController : Controller
    {
        private readonly TiendaDbContext _context;

        public AccountController(TiendaDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u =>
                u.NombreUsuario == model.NombreUsuario &&
                u.Password == model.Password);

            if (usuario == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
            }

            HttpContext.Session.SetString("UsuarioLogueado", usuario.NombreUsuario);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}