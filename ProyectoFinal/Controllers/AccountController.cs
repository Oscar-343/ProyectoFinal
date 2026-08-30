using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.ViewModels;
using System.Linq;

namespace ProyectoFinal.Controllers
{
    // Controlador encargado del inicio y cierre de sesión.
    public class AccountController : Controller
    {
        private readonly TiendaDbContext _context;

        // Se inyecta el DbContext para poder consultar la tabla de usuarios.
        public AccountController(TiendaDbContext context)
        {
            _context = context;
        }

        // Muestra el formulario de login (vacío).
        public IActionResult Login()
        {
            return View();
        }

        // Procesa los datos que el usuario envía desde el formulario de login.
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            var usuario = _context.Usuario.FirstOrDefault(u =>
                u.NombreUsuario == model.NombreUsuario &&
                u.Password == model.Password);

            if (usuario == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
            }

            // Si el usuario es válido, se guarda su nombre en la sesión
            HttpContext.Session.SetString("UsuarioLogueado", usuario.NombreUsuario);

            return RedirectToAction("Index", "Home");
        }

        // Cierra la sesión del usuario y lo regresa al login.
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}