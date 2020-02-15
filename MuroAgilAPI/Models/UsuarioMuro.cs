using System;
using System.Collections.Generic;

namespace MuroAgilAPI.Models {
	public partial class UsuarioMuro {
		public int IdDuenno { get; set; }
		public int IdMuro { get; set; }
		public short Permiso { get; set; }

		public virtual Usuario IdDuennoNavigation { get; set; }
		public virtual Muro IdMuroNavigation { get; set; }
	}
}
