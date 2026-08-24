using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProyectoFinal.Filters
{
    // Filtro personalizado que verifica si el usuario tiene una sesión activa
    public class RequiereSesionAttribute : ActionFilterAttribute
    {
        // Se ejecuta automáticamente ANTES de que corra la acción del controlador.
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var usuario = context.HttpContext.Session.GetString("UsuarioLogueado");

            if (string.IsNullOrEmpty(usuario))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
        }
    }
}