using System.ComponentModel.DataAnnotations;

namespace MuroAgilAPI.Models.Entradas.Muro {
	public class EntEtapaId : EntEtapaBase {
		[Required(ErrorMessage = "Se requiere el ingreso del ID de la etapa.")]
		public int Id { get; set; }
	}
}
