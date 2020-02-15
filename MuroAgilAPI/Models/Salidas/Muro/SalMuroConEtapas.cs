using System.Collections.Generic;

namespace MuroAgilAPI.Models.Salidas.Muro {
	public class SalMuroConEtapas : SalMuroBase {
		public ICollection<SalEtapaBase> Etapas { get; set; }

		public SalMuroConEtapas() {
			Etapas = new HashSet<SalEtapaBase>();
		}
	}
}
