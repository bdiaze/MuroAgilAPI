using System.ComponentModel.DataAnnotations;

namespace MuroAgilAPI.Models.Entradas.UsuarioMuro {
	public class EntPermisoRelacion : EntRelacionMuroUsuario {
		[Required(ErrorMessage = "Requerimos el ingreso del tipo de permiso.")]
		[RegularExpression("2|3", ErrorMessage = "Valor no permitido para el tipo de permiso.")]
		public short Permiso { get; set; }
	}
}
