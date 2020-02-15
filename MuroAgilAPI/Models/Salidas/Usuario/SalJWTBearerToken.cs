using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MuroAgilAPI.Models.Salidas.Usuario {
	public class SalJWTBearerToken {
		public string idToken;
		public int expiresIn;
		
		public SalJWTBearerToken(string idToken, int expiresIn) {
			this.idToken = idToken;
			this.expiresIn = expiresIn;
		}
	}
}
