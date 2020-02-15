using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MuroAgilAPI.Models;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MuroAgilAPI {
	public class Startup {
		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services) {
			byte[] secretKey = Encoding.ASCII.GetBytes(Configuration.GetValue<string>("JWT:SecretKey"));

			services.AddDbContext<MuroAgilContext>();
			services.AddCors();
			services.AddAuthentication(x => {
				x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(x => {
				x.RequireHttpsMetadata = false;
				x.SaveToken = true;
				x.TokenValidationParameters = new TokenValidationParameters {
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(secretKey),
					ValidateIssuer = false,
					ValidateAudience = false
				};
			});

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			services.Configure<ApiBehaviorOptions>(options => {
				options.InvalidModelStateResponseFactory = context => {
					var problemDetails = new ValidationProblemDetails(context.ModelState);
					var result = new BadRequestObjectResult(problemDetails);

					result.ContentTypes.Add("application/problem+json");
					result.ContentTypes.Add("application/problem+xml");

					return result;
				};
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			} else {
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseCors(policy => {
				policy.AllowAnyHeader();
				policy.AllowAnyMethod();
				policy.AllowAnyOrigin();
				policy.AllowCredentials();
			});
			app.UseAuthentication();
			app.UseMvc();
		}
	}
}
