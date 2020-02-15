using MuroAgilAPI.Models.Otros;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuroAgilAPI.Models.Entradas.Muro {
	public class EntMuroEtapa : EntMuroBase {
		[EnsureOneElementValidation(ErrorMessage = "Se requiere el ingreso de por lo menos una etapa.")]
		public ICollection<EntEtapaBase> Etapas { get; set; }
				
		public EntMuroEtapa() {
			Etapas = new List<EntEtapaBase>();
		}
	}
}
