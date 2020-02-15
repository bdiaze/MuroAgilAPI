using MuroAgilAPI.Entradas.Usuario;
using System.ComponentModel.DataAnnotations;

namespace MuroAgilAPI.Models.Entradas.Usuario {
	public class EntCorreoContrasenna : EntDireccionCorreo {
		[Required(ErrorMessage = "Requerimos el ingreso de tu contraseña.")]
		[MinLength(8, ErrorMessage = "Tu contraseña debe tener 8 caracteres como mínimo.")]
		public string Contrasenna { get; set; }
	}
}
