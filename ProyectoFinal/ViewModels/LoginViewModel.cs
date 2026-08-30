using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.ViewModels
{
    // ViewModel que recibe los datos del formulario de inicio de sesión.
    public class LoginViewModel
    {
        // Nombre de usuario que escribe la persona para iniciar sesión.
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string NombreUsuario { get; set; }

        // Contraseña que escribe la persona para iniciar sesión.
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; }
    }
}