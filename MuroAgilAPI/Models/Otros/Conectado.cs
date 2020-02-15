using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MuroAgilAPI.Models.Otros {
	public static class Conectado {
		public static int ID(ControllerBase controllerBase) {
			ClaimsIdentity claims = (ClaimsIdentity)controllerBase.User.Identity;
			return int.Parse(claims.FindFirst(ClaimTypes.NameIdentifier).Value);
		}
	}
}
