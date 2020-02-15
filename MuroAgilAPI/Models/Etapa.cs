using System;
using System.Collections.Generic;

namespace MuroAgilAPI.Models {
	public partial class Etapa {
		public Etapa() {
			Tarea = new HashSet<Tarea>();
		}

		public int Id { get; set; }
		public int IdMuro { get; set; }
		public string Nombre { get; set; }
		public short Posicion { get; set; }

		public virtual Muro IdMuroNavigation { get; set; }
		public virtual ICollection<Tarea> Tarea { get; set; }
	}
}
