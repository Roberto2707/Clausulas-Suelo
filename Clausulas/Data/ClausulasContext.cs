using SQLite.CodeFirst;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace Clausulas
{
    /// <summary>
    /// Clase para la base de datos de Hipotecas
    /// </summary>
    public class ClausulasContext : DbContext
    {

        #region Declaración de Tablas

        public DbSet<Hipoteca> Hipotecas { get; set; }
        public DbSet<Periodo> Periodos { get; set; }
        public DbSet<Amortizacion> Amortizaciones { get; set; }

        #endregion

        #region Constructor de la Clase

        public ClausulasContext() : base("name=ClausulasSqliteConnection")
        {
            // No hace falta hacer nada aquí, ya que todo se hace en OnModelCreating
        }

        #endregion

        #region Sobrecargas

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Propiedades de la tabla de Hipotecas
            modelBuilder.Entity<Hipoteca>().ToTable("Hipotecas");
            modelBuilder.Entity<Hipoteca>().Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Hipoteca>().HasKey(x => x.Id);
            modelBuilder.Entity<Hipoteca>().Property(x => x.Nombre).IsRequired();

            // Propiedades de la tabla de Periodos
            modelBuilder.Entity<Periodo>().ToTable("Periodos");
            modelBuilder.Entity<Periodo>().Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Periodo>().HasKey(x => x.Id);

            // Enlace entre tablas one-to-many 
            modelBuilder.Entity<Periodo>().HasRequired<Hipoteca>(s => s.Hipoteca).WithMany(s => s.Periodos).HasForeignKey(s => s.IdHipoteca).WillCascadeOnDelete();

            // Propiedades de la tabla de Amortizaciones
            modelBuilder.Entity<Amortizacion>().ToTable("Amortizaciones");
            modelBuilder.Entity<Amortizacion>().Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Amortizacion>().HasKey(x => x.Id);

            // Enlace entre tablas one-to-many 
            modelBuilder.Entity<Amortizacion>().HasRequired<Hipoteca>(s => s.Hipoteca).WithMany(s => s.Amortizaciones).HasForeignKey(s => s.IdHipoteca).WillCascadeOnDelete();

            // Creación de la base de datos
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<ClausulasContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }

        #endregion

    }
}
