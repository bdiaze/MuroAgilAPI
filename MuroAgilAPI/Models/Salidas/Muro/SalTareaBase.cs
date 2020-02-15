namespace MuroAgilAPI.Models.Salidas.Muro {
	public class SalTareaBase {
		public int Id { get; set; }
		public string Titulo { get; set; }
		public string Descripcion { get; set; }
		public short Posicion { get; set; }
		public short Familia { get; set; }
		public short Red { get; set; }
		public short Green { get; set; }
		public short Blue { get; set; }
	}
}
