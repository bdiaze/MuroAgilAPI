using System.Collections.Generic;

namespace MuroAgilAPI.Models.Salidas.Muro {
	public class SalMuroConTareas : SalMuroBase {
		public ICollection<SalEtapaConTareas> Etapas { get; set; }

		public SalMuroConTareas() {
			Etapas = new HashSet<SalEtapaConTareas>();
		}
	}
}
