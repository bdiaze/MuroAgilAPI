using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MuroAgilAPI.Models.Otros {
	public class EnviarCorreo {
		private readonly IConfiguration _configuration;
		private readonly string _dominio;

		private const string TEMP_VERIF_DIREC_CORREO = "VerificarDireccionCorreo.html";
		private const string TEMP_RECUP_CONTR = "RecuperarContrasenna.html";

		public EnviarCorreo(IConfiguration configuration, ControllerBase controller) {
			_configuration = configuration;
			_dominio = getDominio(controller.Request.Host.ToString());
		}
		
		public void EnviarCorreoVerificarDireccionCorreo(Usuario usuario) {
			string url = string.Format("https://{0}/Usuario/ValidarCorreo?Correo={1}&TokenVerificador={2}",
				_dominio,
				Uri.EscapeDataString(usuario.Correo),
				Uri.EscapeDataString(usuario.TokenVerificador));

			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("[NOMBRE]", WebUtility.HtmlEncode(usuario.Nombre));
			dictionary.Add("[URL]", url);

			string asunto = "Verificación de Dirección de Correo - Muro Ágil";
			string cuerpo = generarCuerpo(TEMP_VERIF_DIREC_CORREO, dictionary);
			Enviar(usuario.Correo, usuario.Nombre, asunto, cuerpo);
		}

		public void EnviarCorreoRecuperarContrasenna(Usuario usuario) {
			string url = string.Format("https://{0}/Usuario/GenerarNuevaContrasenna?Correo={1}&TokenRecupContr={2}",
				_dominio,
				Uri.EscapeDataString(usuario.Correo),
				Uri.EscapeDataString(usuario.TokenRecupContr));

			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("[NOMBRE]", WebUtility.HtmlEncode(usuario.Nombre));
			dictionary.Add("[URL]", url);

			string asunto = "Recuperación de Contraseña - Muro Ágil";
			string cuerpo = generarCuerpo(TEMP_RECUP_CONTR, dictionary);
			Enviar(usuario.Correo, usuario.Nombre, asunto, cuerpo);
		}

		private async void Enviar(string correoDestinatario, string nombreDestinatario, string asunto, string cuerpo) {
			string nombreAplicacion = _configuration.GetValue<string>("Correo:NombreAplicacion");
			string muroAgilEmail = _configuration.GetValue<string>("Correo:Direccion");
			string muroAgilNombre = _configuration.GetValue<string>("Correo:Nombre");
			string servAccountEmail = _configuration.GetValue<string>("Correo:ServiceAccount:client_email");
			string servAccountPrivKey = _configuration.GetValue<string>("Correo:ServiceAccount:private_key");

			ServiceAccountCredential credential = new ServiceAccountCredential(
				new ServiceAccountCredential.Initializer(servAccountEmail) {
					User = muroAgilEmail,
					Scopes = new[] { GmailService.Scope.GmailSend }
				}.FromPrivateKey(servAccountPrivKey)		
			);

			bool gotAccessToken = await credential.RequestAccessTokenAsync(CancellationToken.None);
			if (gotAccessToken) {
				GmailService service = new GmailService(
					new BaseClientService.Initializer() {
						ApplicationName = nombreAplicacion,
						HttpClientInitializer = credential
					}
				);

				MailAddress fromAddress = new MailAddress(muroAgilEmail, muroAgilNombre, Encoding.UTF8);
				MailAddress toAddress = new MailAddress(correoDestinatario, nombreDestinatario, Encoding.UTF8);
				MailMessage message = new MailMessage(fromAddress, toAddress) {
					Subject = asunto,
					Body = cuerpo,
					SubjectEncoding = Encoding.UTF8,
					HeadersEncoding = Encoding.UTF8,
					BodyEncoding = Encoding.UTF8,
					IsBodyHtml = true
				};

				MimeMessage mimeMessage = MimeMessage.CreateFromMailMessage(message);
				MemoryStream stream = new MemoryStream();
				mimeMessage.WriteTo(stream);

				string rawMessage = Convert.ToBase64String(stream.ToArray())
					.Replace("+", "-")
					.Replace("/", "_")
					.Replace("=", "");

				service.Users.Messages.Send(new Message {
					Raw = rawMessage
				}, muroAgilEmail).Execute();
			}
		}

		private static string generarCuerpo(string template, Dictionary<string, string> dictionary) {
			string pathTemplate = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"TemplatesCorreos", template);
			string[] lineasTemplate = File.ReadAllLines(pathTemplate);

			StringBuilder builder = new StringBuilder();
			foreach (string linea in lineasTemplate) {
				builder.Append(linea);
			}

			string salida = builder.ToString();

			if (dictionary != null) {
				foreach (KeyValuePair<string, string> item in dictionary) {
					salida = salida.Replace(item.Key, item.Value);
				}
			}

			return salida;
		}

		private static string getDominio(string dominio) {
			if (dominio.ToLower().IndexOf("localhost") == -1) {
				if (dominio.ToLower().IndexOf("beta.muroagil.cl") != -1) {
					return "beta.muroagil.cl";
				}
				return "www.muroagil.cl";
			}
			return dominio;
		}
	}
}
