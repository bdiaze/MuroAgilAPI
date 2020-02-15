using System.ComponentModel.DataAnnotations;

namespace MuroAgilAPI.Entradas.Usuario {
	public class EntDireccionCorreo {
		[Required(ErrorMessage = "Requerimos el ingreso de un correo electrónico.")]
		[EmailAddress(ErrorMessage = "El correo electrónico ingresado posee un formato inválido.")]
		public string Correo { get; set; }
	}
}
