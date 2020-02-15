using System.Collections.Generic;

namespace MuroAgilAPI.Models.Salidas.Muro {
	public class SalEtapaConTareas : SalEtapaBase{
		public ICollection<SalTareaBase> Tareas { get; set; }

		public SalEtapaConTareas() {
			Tareas = new HashSet<SalTareaBase>();
		}
	}
}
