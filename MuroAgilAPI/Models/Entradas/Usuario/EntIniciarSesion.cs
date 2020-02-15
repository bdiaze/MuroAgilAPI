using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MuroAgilAPI.Models.Entradas.Usuario {
	public class EntIniciarSesion {
		[Required(ErrorMessage = "Requerimos el ingreso de un correo electrónico.")]
		[EmailAddress(ErrorMessage = "El correo electrónico ingresado posee un formato inválido.")]
		public string Correo { get; set; }

		[Required(ErrorMessage = "Requerimos el ingreso de tu contraseña.")]
		public string Contrasenna { get; set; }
	}
}
