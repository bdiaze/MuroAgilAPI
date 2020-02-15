using MuroAgilAPI.Entradas.Usuario;
using System.ComponentModel.DataAnnotations;

namespace MuroAgilAPI.Models.Entradas.UsuarioMuro {
	public class EntRelacionMuroUsuario : EntDireccionCorreo {
		[Required(ErrorMessage = "Requerimos el ingreso del identificador del muro.")]
		public int IdMuro { get; set; }
	}
}
