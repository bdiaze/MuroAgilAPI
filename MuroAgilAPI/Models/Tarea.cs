using System;
using System.Collections.Generic;

namespace MuroAgilAPI.Models {
	public partial class Tarea {
		public int Id { get; set; }
		public string Descripcion { get; set; }
		public short Familia { get; set; }
		public int IdEtapa { get; set; }
		public short Posicion { get; set; }
		public string Titulo { get; set; }
		public short Blue { get; set; }
		public short Green { get; set; }
		public short Red { get; set; }

		public virtual Etapa IdEtapaNavigation { get; set; }
	}
}
