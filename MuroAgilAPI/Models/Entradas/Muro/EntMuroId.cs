using System.ComponentModel.DataAnnotations;

namespace MuroAgilAPI.Models.Entradas.Muro {
	public class EntMuroId : EntMuroBase {
		[Required(ErrorMessage = "Se requiere el ingreso del ID del muro.")]
		public int Id { get; set; }


		[Required(ErrorMessage = "Se requiere definir el comportamiento para tareas sin etapa.")]
		[Range(1, 3, ErrorMessage = "El comportamiento definido para las tareas sin etapa es inválido.")]
		public short Accion { get; set; }
	}
}
