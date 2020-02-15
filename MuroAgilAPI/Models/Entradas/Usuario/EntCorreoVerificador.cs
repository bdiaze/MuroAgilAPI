using System.ComponentModel.DataAnnotations;

namespace MuroAgilAPI.Entradas.Usuario {
	public class EntCorreoVerificador {
		[Required(ErrorMessage = "Requerimos el ingreso de tu correo electrónico.")]
		[EmailAddress(ErrorMessage = "El correo electrónico ingresado posee un formato inválido.")]
		public string Correo { get; set; }

		[Required(ErrorMessage = "Requerimos el ingreso de tu token de verificación.")]
		public string TokenVerificador { get; set; }
	}
}
