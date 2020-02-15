using System;
using System.Collections.Generic;

namespace MuroAgilAPI.Models {
	public partial class Usuario {
		public Usuario() {
			UsuarioMuro = new HashSet<UsuarioMuro>();
		}

		public int Id { get; set; }
		public string Correo { get; set; }
		public string HashContrasenna { get; set; }
		public string Nombre { get; set; }
		public string TokenVerificador { get; set; }
		public DateTime FechaCreacion { get; set; }
		public DateTime? FechaRecupContr { get; set; }
		public string TokenRecupContr { get; set; }

		public virtual ICollection<UsuarioMuro> UsuarioMuro { get; set; }
	}
}
