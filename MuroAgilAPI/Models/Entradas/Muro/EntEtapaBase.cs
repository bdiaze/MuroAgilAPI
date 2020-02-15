using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MuroAgilAPI.Models.Entradas.Muro {
	public class EntEtapaBase {
		[Required(ErrorMessage = "Se requiere el ingreso del nombre de cada etapa.")]
		public string Nombre { get; set; }

		[Required(ErrorMessage = "Se requiere definir la posición de la etapa.")]
		public short Posicion { get; set; }
	}
}
