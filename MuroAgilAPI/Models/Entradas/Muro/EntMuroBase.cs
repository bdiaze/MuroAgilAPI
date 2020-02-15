using System.ComponentModel.DataAnnotations;

namespace MuroAgilAPI.Models.Entradas.Muro {
	public class EntMuroBase {
		[Required(ErrorMessage = "Se requiere el ingreso del nombre del muro.")]
		public string Nombre { get; set; }
	}
}
