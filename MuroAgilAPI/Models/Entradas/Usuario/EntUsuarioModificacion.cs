using System.ComponentModel.DataAnnotations;

namespace MuroAgilAPI.Models.Entradas.Usuario {
	public class EntUsuarioModificacion {
		[Required(ErrorMessage = "Requerimos el ingreso de tu nombre.")]
		public string Nombre { get; set; }
		
		public string ContrasennaActual { get; set; }
		public string ContrasennaNueva { get; set; }
	}
}
