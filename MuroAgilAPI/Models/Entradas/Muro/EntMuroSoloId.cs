using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuroAgilAPI.Models.Entradas.Muro {
	public class EntMuroSoloId {
		[Required(ErrorMessage = "Se requiere el ingreso del ID del muro.")]
		public int Id { get; set; }

		public ICollection<EntEtapaSoloId> Etapas { get; set; }

		public EntMuroSoloId() {
			Etapas = new List<EntEtapaSoloId>();
		}
	}
}
