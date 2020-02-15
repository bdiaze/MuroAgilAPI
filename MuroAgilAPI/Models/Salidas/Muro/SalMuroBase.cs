namespace MuroAgilAPI.Models.Salidas.Muro {
	public class SalMuroBase {
		public int IdMuro { get; set; }
		public string NombreMuro { get; set; }
		public string FechaCreacion { get; set; }
		public string FechaUltimaModificacion { get; set; }
		public short Permiso { get; set; }
	}
}
