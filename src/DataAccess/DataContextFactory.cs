using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataAccess
{
    /// <inheritdoc />
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        /// <inheritdoc />
        public DataContext CreateDbContext(string[] _)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder.UseSqlite("Data Source=../WebApp/web-exporter.sqlite");

            return new DataContext(optionsBuilder.Options);
        }
    }
}
