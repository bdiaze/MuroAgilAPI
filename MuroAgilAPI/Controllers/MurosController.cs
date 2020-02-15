using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuroAgilAPI.Models;
using MuroAgilAPI.Models.Entradas.Muro;
using MuroAgilAPI.Models.Otros;
using MuroAgilAPI.Models.Salidas.Muro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MuroAgilAPI.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class MurosController : ControllerBase {
		private readonly MuroAgilContext _context;

		public MurosController(MuroAgilContext context) {
			_context = context;
		}

		// GET: api/Muros - Consultar lista de muros visibles por el usuario...
		[HttpGet]
		public async Task<ActionResult<IEnumerable<SalMuroDuenno>>> GetMuro() {
			List<SalMuroDuenno> salida = new List<SalMuroDuenno>();

			// Se obtiene la lista de muros visibles por el usuario...
			List<UsuarioMuro> muros = await _context.UsuarioMuro
				.Include(um => um.IdMuroNavigation)
				.Where(um => um.IdDuenno == Conectado.ID(this))
				.ToListAsync();

			// Por cada muro obtenido, se obtienen los datos de éste...
			foreach (UsuarioMuro usuarioMuro in muros) {
				SalMuroDuenno muroLista = new SalMuroDuenno() {
					IdMuro = usuarioMuro.IdMuro,
					NombreMuro = usuarioMuro.IdMuroNavigation.Nombre,
					Permiso = usuarioMuro.Permiso,
					FechaCreacion = usuarioMuro.IdMuroNavigation.FechaCreacion.ToString("dd/MM/yyyy HH:mm:ss"),
					FechaUltimaModificacion = usuarioMuro.IdMuroNavigation.FechaUltimaModificacion.ToString("dd/MM/yyyy HH:mm:ss")
				};

				// Se obtiene el nombre/correo del dueño del muro visible por el usuario...
				UsuarioMuro duennoMuro = await _context.UsuarioMuro
					.Include(um => um.IdDuennoNavigation)
					.Where(um =>
						um.Permiso == 1 &&
						um.IdMuro == usuarioMuro.IdMuro)
					.FirstOrDefaultAsync();
				if (duennoMuro != null) {
					muroLista.NombreDuenno = duennoMuro.IdDuennoNavigation.Nombre;
					muroLista.CorreoDuenno = duennoMuro.IdDuennoNavigation.Correo;
				}

				salida.Add(muroLista);
			}

			return salida;
		}

		// GET: api/Muros/5 - Consultar detalle de un muro, y sus etapas... 
		[HttpGet("{idMuro}")]
		public async Task<ActionResult<SalMuroConEtapas>> GetMuro(int idMuro) {
			// Se valida que el usuario sea dueño del muro...
			UsuarioMuro usuarioMuro = await _context.UsuarioMuro
				.Include(um => um.IdMuroNavigation)
				.SingleOrDefaultAsync(um =>
					um.IdMuro == idMuro &&
					um.IdDuenno == Conectado.ID(this) &&
					um.Permiso == 1
				);
			if (usuarioMuro == null) {
				ModelState.AddModelError(
						nameof(idMuro),
						"El muro ingresado no existe o no eres dueño de éste.");
				return ValidationProblem();
			}

			// Se crea la salida solicitada...
			SalMuroConEtapas salida = new SalMuroConEtapas() {
				IdMuro = usuarioMuro.IdMuroNavigation.Id,
				NombreMuro = usuarioMuro.IdMuroNavigation.Nombre,
				FechaCreacion = usuarioMuro.IdMuroNavigation.FechaCreacion.ToString("yyyyMMddHHmmss"),
				FechaUltimaModificacion = usuarioMuro.IdMuroNavigation.FechaUltimaModificacion.ToString("yyyyMMddHHmmss"),
				Permiso = usuarioMuro.Permiso
			};

			// Se obtiene una lista de las etapas asociadas al muro...
			ICollection<Etapa> etapas = await _context.Etapa
				.Where(e => e.IdMuro == idMuro)
				.OrderBy(e => e.Posicion)
				.ToListAsync();

			// Se agregan las etapas a la salida...
			foreach (Etapa etapa in etapas) {
				SalEtapaBase etapaBase = new SalEtapaBase() {
					Id = etapa.Id, 
					Nombre = etapa.Nombre,
					Posicion = etapa.Posicion
				};

				salida.Etapas.Add(etapaBase);
			}

			return salida;
		}

		// GET: api/Muros/ObtenerTareas/5 - Consultar detalle de un muro, sus etapas, y sus tareas...
		[Route("[action]/{idMuro}")]
		[HttpGet]
		public async Task<ActionResult<SalMuroConTareas>> ObtenerTareas(int idMuro) {
			// Se valida que el usuario tenga permisos sobre el muro...
			UsuarioMuro usuarioMuro = await _context.UsuarioMuro
				.Include(um => um.IdMuroNavigation)
				.SingleOrDefaultAsync(um =>
					um.IdMuro == idMuro &&
					um.IdDuenno == Conectado.ID(this) &&
					(um.Permiso == 1 ||
					um.Permiso == 2 ||
					um.Permiso == 3)
				);
			if (usuarioMuro == null) {
				ModelState.AddModelError(
						nameof(idMuro),
						"El muro ingresado no existe o no tienes permisos sobre éste.");
				return ValidationProblem();
			}

			// Se crea la salida solicitada...
			SalMuroConTareas salida = new SalMuroConTareas() {
				IdMuro = usuarioMuro.IdMuroNavigation.Id,
				NombreMuro = usuarioMuro.IdMuroNavigation.Nombre,
				FechaCreacion = usuarioMuro.IdMuroNavigation.FechaCreacion.ToString("yyyyMMddHHmmss"),
				FechaUltimaModificacion = usuarioMuro.IdMuroNavigation.FechaUltimaModificacion.ToString("yyyyMMddHHmmss"),
				Permiso = usuarioMuro.Permiso
			};

			// Se obtiene una lista de las etapas asociadas al muro...
			ICollection<Etapa> etapas = await _context.Etapa
				.Where(e => e.IdMuro == idMuro)
				.OrderBy(e => e.Posicion)
				.ToListAsync();

			// Se agregan las etapas a la salida...
			foreach (Etapa etapa in etapas) {
				SalEtapaConTareas etapaConTareas = new SalEtapaConTareas() {
					Id = etapa.Id,
					Nombre = etapa.Nombre,
					Posicion = etapa.Posicion
				};

				// Se obtiene una lista de las tareas asociadas a la etapa...
				ICollection<Tarea> tareas = await _context.Tarea
					.Where(t => t.IdEtapa == etapa.Id)
					.OrderBy(t => t.Posicion)
					.ToListAsync();

				// Se agregan las tareas a la etapa...
				foreach (Tarea tarea in tareas) {
					SalTareaBase tareaBase = new SalTareaBase() {
						Id = tarea.Id,
						Titulo = tarea.Titulo,
						Descripcion = tarea.Descripcion,
						Posicion = tarea.Posicion,
						Familia = tarea.Familia,
						Red = tarea.Red,
						Green = tarea.Green,
						Blue = tarea.Blue
					};

					etapaConTareas.Tareas.Add(tareaBase);
				}

				salida.Etapas.Add(etapaConTareas);
			}

			return salida;
		}

		// PUT: api/Muros/ModificarTareas - Se modifican las tareas de un muro...
		[Route("[action]")]
		[HttpPut]
		public async Task<ActionResult<SalMuroConTareas>> ModificarTareas(EntMuroSoloId entradaMuro) {
			// Se valida que el usuario tenga permisos de edición en el muro...
			UsuarioMuro usuarioMuro = await _context.UsuarioMuro
				.Include(um => um.IdMuroNavigation)
				.SingleOrDefaultAsync(um =>
					um.IdMuro == entradaMuro.Id &&
					um.IdDuenno == Conectado.ID(this) &&
					(um.Permiso == 1 ||
					um.Permiso == 2)
				);
			if (usuarioMuro == null) {
				ModelState.AddModelError(
						nameof(entradaMuro.Id),
						"El muro ingresado no existe o no tienes permisos sobre éste.");
				return ValidationProblem();
			}

			// Se valida que todas las etapas sean parte del muro a editar...
			foreach (EntEtapaSoloId entradaEtapa in entradaMuro.Etapas) {
				Etapa etapa = await _context.Etapa
					.SingleOrDefaultAsync(e => 
						e.Id == entradaEtapa.Id &&
						e.IdMuro == entradaMuro.Id
					);

				if (etapa == null) {
					ModelState.AddModelError(
							nameof(entradaEtapa.Id),
							"Una de las etapas ingresadas no pertenece al muro...");
					return ValidationProblem();
				}
			}

			// Se eliminan las tareas que no son recibidas desde el cliente...
			List<Tarea> tareasExistentes = await _context.Tarea
			.Include(t => t.IdEtapaNavigation)
			.Where(t => t.IdEtapaNavigation.IdMuro == entradaMuro.Id)
			.ToListAsync();

			foreach (Tarea tareaExistente in tareasExistentes) {
				bool existeTarea = false;
				
				foreach (EntEtapaSoloId entradaEtapa in entradaMuro.Etapas) {
					foreach (EntTareaBase entradaTarea in entradaEtapa.Tareas) {
						if (tareaExistente.Id == entradaTarea.Id) {
							existeTarea = true;
							break;
						}
					}
					if (existeTarea) {
						break;
					};
				}

				if (!existeTarea) {
					_context.Tarea.Remove(tareaExistente);
				}
			}

			// Se agregan o modifican el resto de tareas...
			foreach (EntEtapaSoloId entradaEtapa in entradaMuro.Etapas) {
				foreach (EntTareaBase entradaTarea in entradaEtapa.Tareas) {
					Tarea tareaOriginal = await _context.Tarea
						.Include(t => t.IdEtapaNavigation)
						.SingleOrDefaultAsync(t =>
							t.Id == entradaTarea.Id &&
							t.IdEtapaNavigation.IdMuro == entradaMuro.Id
						);

					if (tareaOriginal == null) {
						// Se agregan las tareas que no existen en la base de datos...
						Tarea nuevaTarea = new Tarea() {
							Titulo = entradaTarea.Titulo,
							Descripcion = entradaTarea.Descripcion,
							IdEtapa = entradaEtapa.Id,
							Posicion = entradaTarea.Posicion,
							Familia = entradaTarea.Familia,
							Red = entradaTarea.Red,
							Green = entradaTarea.Green,
							Blue = entradaTarea.Blue
						};
						_context.Tarea.Add(nuevaTarea);
					} else {
						tareaOriginal.Titulo = entradaTarea.Titulo;
						tareaOriginal.Descripcion = entradaTarea.Descripcion;
						tareaOriginal.IdEtapa = entradaEtapa.Id;
						tareaOriginal.Posicion = entradaTarea.Posicion;
						tareaOriginal.Familia = entradaTarea.Familia;
						tareaOriginal.Red = entradaTarea.Red;
						tareaOriginal.Green = entradaTarea.Green;
						tareaOriginal.Blue = entradaTarea.Blue;
						_context.Tarea.Update(tareaOriginal);
					}
				}
			}

			// Se modifica la fecha de última actualización...
			Muro muro = await _context.Muro.SingleOrDefaultAsync(m => m.Id == entradaMuro.Id);
			muro.FechaUltimaModificacion = DateTime.Now;
			await _context.SaveChangesAsync();

			return await ObtenerTareas(entradaMuro.Id);
		}
		
		// POST: api/Muros - Se crea un nuevo muro y sus etapas, y se retorna éste...
		[HttpPost]
		public async Task<ActionResult<SalMuroConEtapas>> PostMuros(EntMuroEtapa entradaMuro) {
			// Se crea el nuevo muro...
			Muro muro = new Muro() {
				Nombre = entradaMuro.Nombre,
				FechaCreacion = DateTime.Now,
				FechaUltimaModificacion = DateTime.Now			
			};
			await _context.Muro.AddAsync(muro);
			await _context.SaveChangesAsync();

			// Se crea la relación usuario/muro...
			UsuarioMuro usuarioMuro = new UsuarioMuro() {
				IdDuenno = Conectado.ID(this),
				IdMuro = muro.Id,
				Permiso = 1
			};
			await _context.UsuarioMuro.AddAsync(usuarioMuro);

			// Se crean las etapas del muro...
			List<Etapa> etapasCreadas = new List<Etapa>();
			foreach (EntEtapaBase entEtapa in entradaMuro.Etapas) {
				Etapa etapa = new Etapa() {
					IdMuro = muro.Id,
					Nombre = entEtapa.Nombre,
					Posicion = entEtapa.Posicion
				};

				await _context.Etapa.AddAsync(etapa);
				etapasCreadas.Add(etapa);
			}

			await _context.SaveChangesAsync();

			SalMuroConEtapas salida = new SalMuroConEtapas() {
				IdMuro = muro.Id,
				NombreMuro = muro.Nombre,
				FechaCreacion = muro.FechaCreacion.ToString("yyyyMMddHHmmss"),
				FechaUltimaModificacion = muro.FechaUltimaModificacion.ToString("yyyyMMddHHmmss"),
				Permiso = usuarioMuro.Permiso
			};

			foreach (Etapa etapaCreada in etapasCreadas) {
				SalEtapaBase salidaEtapa = new SalEtapaBase() {
					Id = etapaCreada.Id,
					Nombre = etapaCreada.Nombre,
					Posicion = etapaCreada.Posicion
				};
				salida.Etapas.Add(salidaEtapa);
			}

			return salida;
		}

		// PUT: api/Muros - Se modifica un muro existente, y sus etapas...
		[HttpPut]
		public async Task<IActionResult> PutMuro(EntMuroIdEtapaId entradaMuro) {
			//Se valida que el usuario es el dueño del muro a modificar...
			UsuarioMuro usuarioMuro = await _context.UsuarioMuro.SingleOrDefaultAsync(um =>
				um.IdDuenno == Conectado.ID(this) &&
				um.IdMuro == entradaMuro.Id &&
				um.Permiso == 1);

			if (usuarioMuro == null) {
				ModelState.AddModelError(
					nameof(entradaMuro.Id),
					"El muro ingresado no existe o no eres dueño de éste.");
				return ValidationProblem();
			}

			Muro muro = await _context.Muro.SingleOrDefaultAsync(m => m.Id == entradaMuro.Id);
			if (muro == null ) {
				ModelState.AddModelError(
					nameof(entradaMuro.Id),
					"El muro ingresado no existe o no eres dueño de éste.");
				return ValidationProblem();
			}

			muro.Nombre = entradaMuro.Nombre;
			muro.FechaUltimaModificacion = DateTime.Now;

			// Se modifican las etapas existentes, y se marcan las etapas a eliminar...
			List<Etapa> etapasEliminadas = new List<Etapa>();
			List<Etapa> etapas = await _context.Etapa
				.Where(e => e.IdMuro == muro.Id)
				.OrderBy(e => e.Posicion)
				.ToListAsync();

			foreach(Etapa etapa in etapas) {
				bool clienteLoElimino = true;
				foreach(EntEtapaId etapaCliente in entradaMuro.Etapas) {
					if (etapa.Id == etapaCliente.Id) {
						etapa.Nombre = etapaCliente.Nombre;
						etapa.Posicion = etapaCliente.Posicion;
						clienteLoElimino = false;
						break;
					}
				}

				if (clienteLoElimino) {
					etapasEliminadas.Add(etapa);
				}
			}

			// Se agregan las etapas nuevas al muro...
			foreach(EntEtapaId etapaCliente in entradaMuro.Etapas) {
				if (etapaCliente.Id == 0) {
					Etapa etapa = new Etapa() {
						IdMuro = muro.Id,
						Nombre = etapaCliente.Nombre,
						Posicion = etapaCliente.Posicion
					};

					await _context.Etapa.AddAsync(etapa);
				}
			}

			await _context.SaveChangesAsync();

			// Se cambian las tareas de etapas eliminadas, a las etapas que corresponden...
			if (entradaMuro.Accion != 3) {
				Etapa etapaDestino = null;
				if (entradaMuro.Accion == 1) {
					etapas = await _context.Etapa
						.Where(e => e.IdMuro == muro.Id)
						.OrderBy(e => e.Posicion)
						.ToListAsync();
				} else {
					etapas = await _context.Etapa
						.Where(e => e.IdMuro == muro.Id)
						.OrderByDescending(e => e.Posicion)
						.ToListAsync();
				}

				foreach(Etapa etapa in etapas) {
					bool seraEliminada = false;
					foreach(Etapa etapaEliminada in etapasEliminadas) {
						if (etapa.Id == etapaEliminada.Id) {
							seraEliminada = true;
							break;
						}
					}

					if (!seraEliminada) {
						etapaDestino = etapa;
						break;
					}
				}

				if (etapaDestino != null) {
					Tarea ultTarea = await _context.Tarea
						.Where(t => t.IdEtapa == etapaDestino.Id)
						.OrderByDescending(t => t.Posicion)
						.FirstOrDefaultAsync();

					List<Tarea> tareasMover = new List<Tarea>();
					foreach(Etapa etapaEliminada in etapasEliminadas) {
						tareasMover.AddRange(await _context.Tarea
							.Where(t => t.IdEtapa == etapaEliminada.Id)
							.OrderBy(t => t.Posicion)
							.ToListAsync());
					}

					short pos = 0;
					if (ultTarea != null) {
						pos = ultTarea.Posicion;
					}

					foreach(Tarea tareaMover in tareasMover) {
						pos++;
						tareaMover.IdEtapa = etapaDestino.Id;
						tareaMover.Posicion = pos;
					}

					await _context.SaveChangesAsync();
				}
			}

			_context.Etapa.RemoveRange(etapasEliminadas);
			await _context.SaveChangesAsync();

			return Ok();
		}

		// DELETE: api/Muros/5 - Se elimina un muro en específico...
		[HttpDelete("{id}")]
		public async Task<ActionResult<Muro>> DeleteMuro(int id) {
			// Se valida que el usuario es el dueño del muro a eliminar...
			UsuarioMuro usuarioMuro = await _context.UsuarioMuro
				.SingleOrDefaultAsync(um =>
					um.IdMuro == id &&
					um.IdDuenno == Conectado.ID(this) &&
					um.Permiso == 1
				);
			if (usuarioMuro == null) {
				ModelState.AddModelError(
						nameof(id),
						"El muro ingresado no existe o no eres dueño de éste.");
				return ValidationProblem();
			}

			// Se elimina el muro seleccionado...
			Muro muro = await _context.Muro.SingleOrDefaultAsync(m => m.Id == id);
			if (muro != null) {
				_context.Muro.Remove(muro);
				await _context.SaveChangesAsync();
			}

			return Ok();
		}
	}
}
