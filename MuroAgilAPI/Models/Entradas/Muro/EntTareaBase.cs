using System.ComponentModel.DataAnnotations;

namespace MuroAgilAPI.Models.Entradas.Muro {
	public class EntTareaBase {
		[Required(ErrorMessage = "Se requiere el ingreso del ID de la tarea.")]
		public int Id { get; set; }

		[Required(ErrorMessage = "Se requiere el ingreso del título de la tarea.")]
		public string Titulo { get; set; }

		[Required(ErrorMessage = "Se requiere el ingreso de la descripción de la tarea.")]
		public string Descripcion { get; set; }

		[Required(ErrorMessage = "Se requiere el ingreso de la posición de la tarea.")]
		public short Posicion { get; set; }

		[Required(ErrorMessage = "Se requiere el ingreso de la familia de la tarea.")]
		public short Familia { get; set; }

		[Required(ErrorMessage = "Se requiere el ingreso del valor RED de la tarea.")]
		public short Red { get; set; }

		[Required(ErrorMessage = "Se requiere el ingreso del valor GREEN de la tarea.")]
		public short Green { get; set; }

		[Required(ErrorMessage = "Se requiere el ingreso del valor BLUE de la tarea.")]
		public short Blue { get; set; }
	}
}
