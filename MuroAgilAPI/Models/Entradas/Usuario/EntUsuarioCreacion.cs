using MuroAgilAPI.Models.Entradas.Usuario;
using System.ComponentModel.DataAnnotations;

namespace MuroAgilAPI.Entradas.Usuario {
	public class EntUsuarioCreacion : EntCorreoContrasenna {
		[Required(ErrorMessage = "Requerimos el ingreso de tu nombre.")]
		public string Nombre { get; set; }
	}
}
