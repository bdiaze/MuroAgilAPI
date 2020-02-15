using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MuroAgilAPI.Models {
	public partial class MuroAgilContext : DbContext {
		public MuroAgilContext() {
		}

		public MuroAgilContext(DbContextOptions<MuroAgilContext> options)
			: base(options) {
		}

		public virtual DbSet<Etapa> Etapa { get; set; }
		public virtual DbSet<Muro> Muro { get; set; }
		public virtual DbSet<Tarea> Tarea { get; set; }
		public virtual DbSet<Usuario> Usuario { get; set; }
		public virtual DbSet<UsuarioMuro> UsuarioMuro { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			if (!optionsBuilder.IsConfigured) {
				var builder = new ConfigurationBuilder()
					.SetBasePath(Directory.GetCurrentDirectory())
					.AddJsonFile("appsettings.json");
				var configuration = builder.Build();
				optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

			modelBuilder.Entity<Etapa>(entity => {
				entity.ToTable("ETAPA");

				entity.HasIndex(e => e.IdMuro);

				entity.Property(e => e.Id).HasColumnName("ID");

				entity.Property(e => e.IdMuro).HasColumnName("ID_MURO");

				entity.Property(e => e.Nombre)
					.IsRequired()
					.HasColumnName("NOMBRE")
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.Posicion).HasColumnName("POSICION");

				entity.HasOne(d => d.IdMuroNavigation)
					.WithMany(p => p.Etapa)
					.HasForeignKey(d => d.IdMuro)
					.HasConstraintName("FK__ETAPA__ID_MURO__22401542");
			});

			modelBuilder.Entity<Muro>(entity => {
				entity.ToTable("MURO");

				entity.Property(e => e.Id).HasColumnName("ID");

				entity.Property(e => e.FechaCreacion)
					.HasColumnName("FECHA_CREACION")
					.HasColumnType("datetime");

				entity.Property(e => e.FechaUltimaModificacion)
					.HasColumnName("FECHA_ULTIMA_MODIFICACION")
					.HasColumnType("datetime");

				entity.Property(e => e.Nombre)
					.IsRequired()
					.HasColumnName("NOMBRE")
					.HasMaxLength(255)
					.IsUnicode(false);
			});

			modelBuilder.Entity<Tarea>(entity => {
				entity.ToTable("TAREA");

				entity.HasIndex(e => e.IdEtapa);

				entity.Property(e => e.Id).HasColumnName("ID");

				entity.Property(e => e.Descripcion)
					.IsRequired()
					.HasColumnName("DESCRIPCION")
					.IsUnicode(false);

				entity.Property(e => e.Familia).HasColumnName("FAMILIA");

				entity.Property(e => e.IdEtapa).HasColumnName("ID_ETAPA");

				entity.Property(e => e.Posicion).HasColumnName("POSICION");

				entity.Property(e => e.Red).HasColumnName("RED");

				entity.Property(e => e.Green).HasColumnName("GREEN");

				entity.Property(e => e.Blue).HasColumnName("BLUE");

				entity.Property(e => e.Titulo)
					.IsRequired()
					.HasColumnName("TITULO")
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.HasOne(d => d.IdEtapaNavigation)
					.WithMany(p => p.Tarea)
					.HasForeignKey(d => d.IdEtapa)
					.HasConstraintName("FK__TAREA__ID_ETAPA__251C81ED");
			});

			modelBuilder.Entity<Usuario>(entity => {
				entity.ToTable("USUARIO");

				entity.HasIndex(e => e.Correo)
					.HasName("UQ__USUARIO__CC87E1260BB751F7")
					.IsUnique();

				entity.Property(e => e.Id).HasColumnName("ID");

				entity.Property(e => e.Correo)
					.IsRequired()
					.HasColumnName("CORREO")
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.FechaCreacion)
					.HasColumnName("FECHA_CREACION")
					.HasColumnType("datetime")
					.HasDefaultValueSql("('0001-01-01T00:00:00.000')");

				entity.Property(e => e.FechaRecupContr)
					.HasColumnName("FECHA_RECUP_CONTR")
					.HasColumnType("datetime");

				entity.Property(e => e.HashContrasenna)
					.IsRequired()
					.HasColumnName("HASH_CONTRASENNA")
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.Nombre)
					.IsRequired()
					.HasColumnName("NOMBRE")
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.TokenRecupContr)
					.HasColumnName("TOKEN_RECUP_CONTR")
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.TokenVerificador)
					.HasColumnName("TOKEN_VERIFICADOR")
					.HasMaxLength(255)
					.IsUnicode(false);
			});

			modelBuilder.Entity<UsuarioMuro>(entity => {
				entity.HasKey(e => new { e.IdDuenno, e.IdMuro });

				entity.ToTable("USUARIO_MURO");

				entity.HasIndex(e => e.IdMuro);

				entity.Property(e => e.IdDuenno).HasColumnName("ID_DUENNO");

				entity.Property(e => e.IdMuro).HasColumnName("ID_MURO");

				entity.Property(e => e.Permiso).HasColumnName("PERMISO");

				entity.HasOne(d => d.IdDuennoNavigation)
					.WithMany(p => p.UsuarioMuro)
					.HasForeignKey(d => d.IdDuenno)
					.HasConstraintName("FK__USUARIO_M__ID_DU__1E6F845E");

				entity.HasOne(d => d.IdMuroNavigation)
					.WithMany(p => p.UsuarioMuro)
					.HasForeignKey(d => d.IdMuro)
					.HasConstraintName("FK__USUARIO_M__ID_MU__1F63A897");
			});
		}
	}
}
