using System;
using System.Collections.Generic;

namespace MuroAgilAPI.Models {
	public partial class Muro {
		public Muro() {
			Etapa = new HashSet<Etapa>();
			UsuarioMuro = new HashSet<UsuarioMuro>();
		}

		public int Id { get; set; }
		public DateTime FechaCreacion { get; set; }
		public DateTime FechaUltimaModificacion { get; set; }
		public string Nombre { get; set; }

		public virtual ICollection<Etapa> Etapa { get; set; }
		public virtual ICollection<UsuarioMuro> UsuarioMuro { get; set; }
	}
}
