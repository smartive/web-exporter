using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    /// <summary>
    /// Data-context for the web-exporter application.
    /// </summary>
    /// <inheritdoc />
    public class DataContext : DbContext
    {
        /// <summary>
        /// Create a new instance of the data-context.
        /// </summary>
        /// <param name="options">Options for the data-context.</param>
        public DataContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Table of the request-headers.
        /// </summary>
        public DbSet<RequestHeader> RequestHeaders { get; set; }

        /// <summary>
        /// Table of the metric labels.
        /// </summary>
        public DbSet<Label> Labels { get; set; }

        /// <summary>
        /// Table of the response tests.
        /// </summary>
        public DbSet<ResponseTest> ResponseTests { get; set; }

        /// <summary>
        /// Table of the web-checks.
        /// </summary>
        public DbSet<WebCheck> WebChecks { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<WebCheck>()
                .HasMany(wc => wc.RequestHeaders)
                .WithOne(h => h.WebCheck);

            modelBuilder
                .Entity<WebCheck>()
                .HasMany(wc => wc.Labels)
                .WithOne(l => l.WebCheck);

            modelBuilder
                .Entity<WebCheck>()
                .HasMany(wc => wc.ResponseTests)
                .WithOne(l => l.WebCheck);
        }
    }
}
