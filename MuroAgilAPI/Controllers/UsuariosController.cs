using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MuroAgilAPI.Entradas.Usuario;
using MuroAgilAPI.Models;
using MuroAgilAPI.Models.Entradas.Usuario;
using MuroAgilAPI.Models.Otros;
using MuroAgilAPI.Models.Salidas.Usuario;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MuroAgilAPI.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class UsuariosController : ControllerBase {
		private readonly MuroAgilContext _context;
		private readonly IConfiguration _configuration;

		private readonly int _tiempoValidacionUsuarioMinutos;
		private readonly int _tiempoRecuperacionContrasennaMinutos;

		public UsuariosController(MuroAgilContext context, IConfiguration configuration) {
			_context = context;
			_configuration = configuration;

			_tiempoValidacionUsuarioMinutos = _configuration.GetValue<int>("General:TiempoValidacionUsuarioMinutos");
			_tiempoRecuperacionContrasennaMinutos = _configuration.GetValue<int>("General:TiempoRecuperacionContrasennaMinutos");
		}

		// GET: api/Usuarios - Se obtiene datos del usuario conectado...
		[HttpGet]
		public async Task<ActionResult<SalObtenerDatosUsuario>> GetUsuario() {
			Usuario usuario = await _context.Usuario
				.SingleOrDefaultAsync(u => u.Id == Conectado.ID(this));

			SalObtenerDatosUsuario salida = new SalObtenerDatosUsuario();
			if (usuario != null) {
				salida.nombre = usuario.Nombre;
				salida.correo = usuario.Correo;
			}

			return salida;
		}

		// POST: api/Usuarios - Se crea un nuevo usuario...
		[AllowAnonymous, HttpPost]
		public async Task<IActionResult> PostUsuario(EntUsuarioCreacion entrada) {
			// Se eliminan los usuarios que no validaron sus correos después de un día...
			List<Usuario> usuariosSinConfirmar = await _context.Usuario
				.Where(u =>
					u.TokenVerificador != null &&
					u.TokenVerificador.Length > 0 &&
					u.FechaCreacion.AddMinutes(_tiempoValidacionUsuarioMinutos).CompareTo(DateTime.Now) <= 0)
				.ToListAsync();
			_context.Usuario.RemoveRange(usuariosSinConfirmar);
			await _context.SaveChangesAsync();

			// Se valida que el correo no se encuentre registrado...
			entrada.Correo = entrada.Correo.Trim();
			Usuario usuarioExistente = await _context.Usuario
				.SingleOrDefaultAsync(u => u.Correo.Equals(entrada.Correo));
			if (usuarioExistente != null) {
				ModelState.AddModelError(
					nameof(EntUsuarioCreacion.Correo),
					"El correo ingresado ya se encuentra registrado en nuestros sistemas.");
				return ValidationProblem();
			}

			Usuario nuevoUsuario = new Usuario();

			// Se genera token para verificar correo electrónico...
			byte[] randomBytes = new byte[72];
			using (var rng = RandomNumberGenerator.Create()) {
				rng.GetBytes(randomBytes);
			}

			// Se genera hash de la contraseña...
			var hasher = new PasswordHasher<Usuario>();
			string hashPassword = hasher.HashPassword(nuevoUsuario, entrada.Contrasenna);

			// Se genera el nuevo usuario...
			nuevoUsuario.Correo = entrada.Correo;
			nuevoUsuario.Nombre = entrada.Nombre.Trim();
			nuevoUsuario.HashContrasenna = hashPassword;
			nuevoUsuario.FechaCreacion = DateTime.Now;
			nuevoUsuario.TokenVerificador = Convert.ToBase64String(randomBytes);
			await _context.Usuario.AddAsync(nuevoUsuario);
			await _context.SaveChangesAsync();

			EnviarCorreo enviarCorreo = new EnviarCorreo(_configuration, this);
			enviarCorreo.EnviarCorreoVerificarDireccionCorreo(nuevoUsuario);

			return Ok();
		}

		// PUT: api/Usuarios - Se modifica el nombre, o contraseña de un usuario...
		[HttpPut]
		public async Task<IActionResult> PutUsuario(EntUsuarioModificacion entrada) {
			// Se obtiene el usuario del cliente conectado...
			Usuario usuario = await _context.Usuario
				.SingleOrDefaultAsync(u =>
					u.Id == Conectado.ID(this));

			if (entrada.ContrasennaActual != null && entrada.ContrasennaNueva != null) {
				// Si ingresó la contraseña actual y la contraseña nueva...
				// Se valida el largo de la contraseña nueva...
				if (entrada.ContrasennaNueva.Length < 8) {
					ModelState.AddModelError(
						nameof(EntUsuarioModificacion.ContrasennaNueva),
						"La nueva contraseña requiere tener 8 caracteres como mínimo.");
					return ValidationProblem();
				}

				// Se valida que la contraseña actual sea correcta...
				var hasher = new PasswordHasher<Usuario>();
				var result = hasher.VerifyHashedPassword(usuario, usuario.HashContrasenna, entrada.ContrasennaActual);
				if (result != PasswordVerificationResult.Success) {
					ModelState.AddModelError(
						nameof(EntUsuarioModificacion.ContrasennaActual),
						"La contraseña actual ingresada no es correcta.");
					return ValidationProblem();
				}

				// Se genera el hash de la nueva contraseña...
				usuario.HashContrasenna = hasher.HashPassword(usuario, entrada.ContrasennaNueva);
			} else {
				if (entrada.ContrasennaActual != null ^ entrada.ContrasennaNueva != null) {
					// Si solo ingreso la contraseña actual o la contraseña nueva...
					if (entrada.ContrasennaActual == null) {
						ModelState.AddModelError(
							nameof(EntUsuarioModificacion.ContrasennaActual),
							"Para actualizar tu contraseña, debes ingresar tu contraseña actual.");
						return ValidationProblem();
					} else {
						ModelState.AddModelError(
							nameof(EntUsuarioModificacion.ContrasennaNueva),
							"Para actualizar tu contraseña, debes ingresar tu nueva contraseña.");
						return ValidationProblem();
					}
				}
			}

			// Se modifica el nombre del usuario...
			usuario.Nombre = entrada.Nombre.Trim();
			await _context.SaveChangesAsync();

			return Ok();
		}

		// POST: api/Usuarios/EnviarCorreoVerificacion - Se solicita el reenvío del correo de verificación...
		[Route("[action]")]
		[AllowAnonymous, HttpPost]
		public async Task<IActionResult> EnviarCorreoVerificacion(EntDireccionCorreo correo) {
			// Se valida que el correo requiere la verificación de correo...
			Usuario usuario = await _context.Usuario
				.SingleOrDefaultAsync(u => u.Correo.Equals(correo.Correo));
			if (usuario == null || usuario.TokenVerificador == null || usuario.TokenVerificador.Length == 0) {
				ModelState.AddModelError(
					nameof(EntDireccionCorreo.Correo),
					"No se requiere verificar el correo electrónico ingresado.");
				return ValidationProblem();
			}

			EnviarCorreo enviarCorreo = new EnviarCorreo(_configuration, this);
			enviarCorreo.EnviarCorreoVerificarDireccionCorreo(usuario);

			return Ok();
		}

		// PUT: api/Usuarios/ValidarCorreo - Se valida un correo electrónico...
		[Route("[action]")]
		[AllowAnonymous, HttpPut]
		public async Task<IActionResult> ValidarCorreo(EntCorreoVerificador correoVerificador) {
			// Se valida que el token de verificación corresponda al correo...
			Usuario usuario = await _context.Usuario
				.SingleOrDefaultAsync(u =>
					u.Correo.Equals(correoVerificador.Correo) &&
					u.TokenVerificador.Equals(correoVerificador.TokenVerificador));
			if (usuario == null) {
				ModelState.AddModelError(
					nameof(EntCorreoVerificador.Correo),
					"Ocurrió un error al verificar su correo electrónico.");
				return ValidationProblem();
			}

			// Se elimina el token para representar la correcta validación del correo...
			usuario.TokenVerificador = null;
			await _context.SaveChangesAsync();

			return Ok();
		}

		// POST: api/Usuarios/RecuperarContrasenna - Se solicita la recuperación de contraseña de un usuario...
		[Route("[action]")]
		[AllowAnonymous, HttpPost]
		public async Task<IActionResult> RecuperarContrasenna(EntDireccionCorreo correo) {
			// Se valida que el correo este registrado...
			Usuario usuario = await _context.Usuario
				.SingleOrDefaultAsync(u => u.Correo.Equals(correo.Correo));
			if (usuario == null) {
				ModelState.AddModelError(
					nameof(EntDireccionCorreo.Correo),
					"El correo ingresado no se encuentra registrado en nuestro sistema.");
				return ValidationProblem();
			}

			// Se genera token para recuperación de contraseña...
			byte[] randomBytes = new byte[72];
			using (var rng = RandomNumberGenerator.Create()) {
				rng.GetBytes(randomBytes);
			}

			// Se graba el token de recuperación...
			usuario.FechaRecupContr = DateTime.Now;
			usuario.TokenRecupContr = Convert.ToBase64String(randomBytes);
			await _context.SaveChangesAsync();

			EnviarCorreo enviarCorreo = new EnviarCorreo(_configuration, this);
			enviarCorreo.EnviarCorreoRecuperarContrasenna(usuario);
			return Ok();
		}

		// PUT: api/Usuarios/GenerarNuevaContrasenna - Se genera una nueva contraseña...
		[Route("[action]")]
		[AllowAnonymous, HttpPut]
		public async Task<IActionResult> GenerarNuevaContrasenna(EntCorreoRecuperador correoRecuperador) {
			// Se eliminan los token de recuperación para los usuarios fuera del tiempo límite...
			List<Usuario> usuariosSinRecuperar = await _context.Usuario
				.Where(u =>
					u.TokenRecupContr != null &&
					u.TokenRecupContr.Length > 0 &&
					u.FechaRecupContr != null &&
					u.FechaRecupContr.GetValueOrDefault().AddMinutes(_tiempoRecuperacionContrasennaMinutos).CompareTo(DateTime.Now) <= 0)
				.ToListAsync();
			foreach (Usuario usuarioSinRecuperar in usuariosSinRecuperar) {
				usuarioSinRecuperar.FechaRecupContr = null;
				usuarioSinRecuperar.TokenRecupContr = null;
			}
			await _context.SaveChangesAsync();

			// Se valida que el token de recuperación corresponda al correo...
			Usuario usuario = await _context.Usuario
				.SingleOrDefaultAsync(u =>
					u.Correo.Equals(correoRecuperador.Correo) &&
					u.TokenRecupContr.Equals(correoRecuperador.TokenRecupContr));
			if (usuario == null) {
				ModelState.AddModelError(
					nameof(EntCorreoRecuperador.Correo),
					"Ocurrió un error al generar su nueva contraseña.");
				return ValidationProblem();
			}

			// Se cambia la contraseña del usuario...
			var hasher = new PasswordHasher<Usuario>();
			usuario.HashContrasenna = hasher.HashPassword(usuario, correoRecuperador.Contrasenna);
			usuario.FechaRecupContr = null;
			usuario.TokenRecupContr = null;

			await _context.SaveChangesAsync();

			return Ok();
		}

		// POST: api/Usuarios/IniciarSesion - Se solicita el inicio de sesión...
		[Route("[action]")]
		[AllowAnonymous, HttpPost]
		public async Task<ActionResult<SalJWTBearerToken>> IniciarSesion(EntIniciarSesion entrada) {
			// Se valida que exista el usuario...
			Usuario usuario = await _context.Usuario.
				SingleOrDefaultAsync(u => u.Correo.Equals(entrada.Correo));
			if (usuario == null) {
				ModelState.AddModelError(
					nameof(EntCorreoContrasenna.Correo),
					"El correo electrónico o la contraseña ingresada no es correcto.");
				return ValidationProblem();
			}

			// Se valida que la contraseña ingresada sea correcta...
			var hasher = new PasswordHasher<Usuario>();
			var result = hasher.VerifyHashedPassword(usuario, usuario.HashContrasenna, entrada.Contrasenna);
			if (result != PasswordVerificationResult.Success) {
				ModelState.AddModelError(
					nameof(EntCorreoContrasenna.Correo),
					"El correo electrónico o la contraseña ingresada no es correcto.");
				return ValidationProblem();
			}

			// Se valida que el usuario haya validado su correo electrónico...
			if (usuario.TokenVerificador != null && usuario.TokenVerificador.Length > 0) {
				ModelState.AddModelError(
					nameof(EntCorreoContrasenna.Correo),
					"Para iniciar sesión, es necesario validar su correo electrónico.");
				return ValidationProblem();
			}

			// Se genera nuestro token JWT...
			byte[] secretKey = Encoding.ASCII.GetBytes(_configuration.GetValue<string>("JWT:SecretKey"));
			int expiration = _configuration.GetValue<int>("JWT:ExpirationMinutos");

			ClaimsIdentity claims = new ClaimsIdentity(new[] {
				new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString())
			});

			SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor {
				Subject = claims,
				Expires = DateTime.UtcNow.AddMinutes(expiration),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
			};

			JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
			SecurityToken createdToken = tokenHandler.CreateToken(tokenDescriptor);

			SalJWTBearerToken salJWT = new SalJWTBearerToken(tokenHandler.WriteToken(createdToken), expiration);

			return salJWT;
		}
	}
}
