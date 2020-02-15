using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuroAgilAPI.Models;
using MuroAgilAPI.Models.Entradas.UsuarioMuro;
using MuroAgilAPI.Models.Otros;
using MuroAgilAPI.Models.Salidas.UsuarioMuro;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MuroAgilAPI.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class UsuarioMurosController : ControllerBase {
		private readonly MuroAgilContext _context;

		public UsuarioMurosController(MuroAgilContext context) {
			_context = context;
		}

		// GET: api/UsuarioMuros/5 - Retorna la lista de usuarios que tienen permisos sobre un muro...
		[HttpGet("{idMuro}")]
		public async Task<ActionResult<IEnumerable<SalPermisoLista>>> GetUsuarioMuro(int idMuro) {
			List<SalPermisoLista> salida = new List<SalPermisoLista>();

			// Se valida que el usuario es el dueño del muro...
			UsuarioMuro duennoMuro = await _context.UsuarioMuro
				.SingleOrDefaultAsync(um =>
					um.IdMuro == idMuro &&
					um.IdDuenno == Conectado.ID(this) &&
					um.Permiso == 1);
			if (duennoMuro == null) {
				ModelState.AddModelError(
						nameof(idMuro),
						"El muro ingresado no existe o no eres dueño de éste.");
				return ValidationProblem();
			}

			// Se obtiene los usuarios que tienen permisos sobre el muro...
			List<UsuarioMuro> usuariosMuro = await _context.UsuarioMuro
				.Include(um => um.IdDuennoNavigation)
				.Where(um => um.IdMuro == idMuro)
				.ToListAsync();

			// Se agregan los elementos a la salida...
			foreach (UsuarioMuro usuarioMuro in usuariosMuro) {
				SalPermisoLista permiso = new SalPermisoLista() {
					IdUsuario = usuarioMuro.IdDuenno,
					NombreUsuario = usuarioMuro.IdDuennoNavigation.Nombre,
					CorreoUsuario = usuarioMuro.IdDuennoNavigation.Correo,
					Permiso = usuarioMuro.Permiso
				};

				salida.Add(permiso);
			}

			return salida;
		}

		// POST: api/UsuarioMuros - Se agrega un permiso sobre un muro, y se retorna...
		[HttpPost]
		public async Task<ActionResult<SalPermisoLista>> PostUsuarioMuro(EntPermisoRelacion permisoRelacion) {
			permisoRelacion.Correo = permisoRelacion.Correo.Trim();
			
			// Se valida que el muro pertenezca al usuario que inició sesión...
			UsuarioMuro usuarioMuro = await _context.UsuarioMuro
				.SingleOrDefaultAsync(um =>
					um.IdMuro == permisoRelacion.IdMuro &&
					um.IdDuenno == Conectado.ID(this) &&
					um.Permiso == 1
				);
			if (usuarioMuro == null) {
				ModelState.AddModelError(
						nameof(permisoRelacion.IdMuro),
						"El muro ingresado no existe, o no te pertenece.");
				return ValidationProblem();
			}

			// Se valida que el correo ingresado este registrado, y no sea el mismo del usuario que inició sesión...
			Usuario usuario = await _context.Usuario.SingleOrDefaultAsync(u => 
				u.Correo == permisoRelacion.Correo &&
				u.Id != Conectado.ID(this));
			if (usuario == null) {
				ModelState.AddModelError(
						nameof(permisoRelacion.Correo),
						"El correo ingresado no es válido.");
				return ValidationProblem();
			}

			// Se valida que la relación Muro/Usuario no existiese de antes...
			usuarioMuro = await _context.UsuarioMuro
				.SingleOrDefaultAsync(um =>
					um.IdMuro == permisoRelacion.IdMuro &&
					um.IdDuenno == usuario.Id
				);
			if (usuarioMuro != null) {
				ModelState.AddModelError(
						nameof(permisoRelacion.Correo),
						"El usuario ingresado ya tiene permisos sobre el muro.");
				return ValidationProblem();
			}

			// Se crea la nueva relación...
			usuarioMuro = new UsuarioMuro {
				IdDuenno = usuario.Id,
				IdMuro = permisoRelacion.IdMuro,
				Permiso = permisoRelacion.Permiso
			};
			await _context.UsuarioMuro.AddAsync(usuarioMuro);
			await _context.SaveChangesAsync();

			SalPermisoLista salida = new SalPermisoLista() {
				IdUsuario = usuarioMuro.IdDuenno,
				NombreUsuario = usuario.Nombre,
				CorreoUsuario = usuario.Correo,
				Permiso = usuarioMuro.Permiso
			};

			return salida;
		}

		// PUT: api/UsuarioMuros - Se modifica un permiso sobre un muro, y se retorna...
		[HttpPut]
		public async Task<ActionResult<SalPermisoLista>> PutUsuarioMuro(EntPermisoRelacion permisoRelacion) {
			permisoRelacion.Correo = permisoRelacion.Correo.Trim();
			
			// Se valida que el muro pertenezca al usuario que inició sesión...
			UsuarioMuro usuarioMuro = await _context.UsuarioMuro
				.SingleOrDefaultAsync(um =>
					um.IdMuro == permisoRelacion.IdMuro &&
					um.IdDuenno == Conectado.ID(this) &&
					um.Permiso == 1
				);
			if (usuarioMuro == null) {
				ModelState.AddModelError(
						nameof(permisoRelacion.IdMuro),
						"El muro ingresado no existe, o no te pertenece.");
				return ValidationProblem();
			}

			// Se valida que exista la relación que se está tratando de modificar...
			usuarioMuro = await _context.UsuarioMuro
				.Include(um => um.IdDuennoNavigation)
				.SingleOrDefaultAsync(um =>
					um.IdDuennoNavigation.Correo == permisoRelacion.Correo &&
					um.IdMuro == permisoRelacion.IdMuro &&
					um.Permiso != 1
				);
			if (usuarioMuro == null) {
				ModelState.AddModelError(
						nameof(permisoRelacion.Correo),
						"El permiso que se quiere modificar no existe.");
				return ValidationProblem();
			}

			// Se cambia el permiso de la relación...
			usuarioMuro.Permiso = permisoRelacion.Permiso;
			await _context.SaveChangesAsync();

			SalPermisoLista salida = new SalPermisoLista() {
				IdUsuario = usuarioMuro.IdDuenno,
				NombreUsuario = usuarioMuro.IdDuennoNavigation.Nombre,
				CorreoUsuario = usuarioMuro.IdDuennoNavigation.Correo,
				Permiso = usuarioMuro.Permiso
			};

			return salida;
		}

		// DELETE: api/UsuarioMuros/5/ejemplo@ejemplo.com - Se elimina un permiso de un muro...
		[HttpDelete("{idMuro}/{correo}")]
		public async Task<IActionResult> DeleteUsuarioMuro(int idMuro, string correo) {
			correo = correo.Trim();

			// Se valida que el muro pertenezca al usuario que inició sesión...
			UsuarioMuro usuarioMuro = await _context.UsuarioMuro
				.SingleOrDefaultAsync(um =>
					um.IdMuro == idMuro &&
					um.IdDuenno == Conectado.ID(this) &&
					um.Permiso == 1
				);
			if (usuarioMuro == null) {
				ModelState.AddModelError(
						nameof(idMuro),
						"El muro ingresado no existe, o no te pertenece.");
				return ValidationProblem();
			}

			// Se valida que exista la relación que se está tratando de eliminar...
			usuarioMuro = await _context.UsuarioMuro
				.Include(um => um.IdDuennoNavigation)
				.SingleOrDefaultAsync(um =>
					um.IdDuennoNavigation.Correo == correo &&
					um.IdMuro == idMuro &&
					um.Permiso != 1
				);
			if (usuarioMuro == null) {
				ModelState.AddModelError(
						nameof(correo),
						"El permiso que se quiere eliminar no existe.");
				return ValidationProblem();
			}

			// Se elimina la relación...
			_context.UsuarioMuro.Remove(usuarioMuro);
			await _context.SaveChangesAsync();

			return Ok();
		}

		// DELETE: api/UsuarioMuros/Renunciar/5 - Se elimina el permiso que uno tienen sobre un muro...
		[Route("[action]/{idMuro}")]
		[HttpDelete]
		public async Task<IActionResult> Renunciar(int idMuro) {
			// Se valida que exista la relación, y el usuario no sea dueño...
			UsuarioMuro usuarioMuro = await _context.UsuarioMuro
				.SingleOrDefaultAsync(um =>
					um.IdDuenno == Conectado.ID(this) &&
					um.IdMuro == idMuro &&
					um.Permiso != 1
				);
			if (usuarioMuro == null) {
				ModelState.AddModelError(
						nameof(idMuro),
						"No tienes permisos sobre el muro seleccionado.");
				return ValidationProblem();
			}

			// Se elimina el permiso del usuario...
			_context.UsuarioMuro.Remove(usuarioMuro);
			await _context.SaveChangesAsync();

			return Ok();
		}
	}
}
