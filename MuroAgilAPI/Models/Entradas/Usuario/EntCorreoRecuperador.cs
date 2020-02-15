using MuroAgilAPI.Models.Entradas.Usuario;
using System.ComponentModel.DataAnnotations;

namespace MuroAgilAPI.Entradas.Usuario {
	public class EntCorreoRecuperador : EntCorreoContrasenna {
		[Required(ErrorMessage = "Requerimos el ingreso de tu token de recuperación de contraseña.")]
		public string TokenRecupContr { get; set; }
	}
}
