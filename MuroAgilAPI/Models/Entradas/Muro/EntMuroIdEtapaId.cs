using MuroAgilAPI.Models.Otros;
using System.Collections.Generic;

namespace MuroAgilAPI.Models.Entradas.Muro {
	public class EntMuroIdEtapaId : EntMuroId {
		[EnsureOneElementValidation(ErrorMessage = "Se requiere el ingreso de por lo menos una etapa.")]
		public ICollection<EntEtapaId> Etapas { get; set; }

		public EntMuroIdEtapaId() {
			Etapas = new List<EntEtapaId>();
		}
	}
}
