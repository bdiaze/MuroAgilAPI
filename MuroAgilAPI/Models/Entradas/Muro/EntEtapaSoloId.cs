using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuroAgilAPI.Models.Entradas.Muro {
	public class EntEtapaSoloId {
		[Required(ErrorMessage = "Se requiere el ingreso del ID de la etapa.")]
		public int Id { get; set; }

		public ICollection<EntTareaBase> Tareas { get; set; }

		public EntEtapaSoloId() {
			Tareas = new List<EntTareaBase>();
		}
	}
}
