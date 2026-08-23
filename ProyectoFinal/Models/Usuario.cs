using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models
{
    public class Usuario
    {
        [Key]
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreUsuario { get; set; }

        [Required]
        public string Password { get; set; }
    }
}