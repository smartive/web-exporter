using DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using Smartive.Core.Database.Extensions;

namespace DataAccess
{
    /// <summary>
    /// Extension-methods for the data access library.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Add data access repositories to the service collection.
        /// </summary>
        /// <param name="services">DI collection</param>
        /// <returns>Same DI collection for chaining.</returns>
        public static IServiceCollection AddDataAccess(this IServiceCollection services)
        {
            return services
                .AddRepository<Label, DataContext>()
                .AddRepository<RequestHeader, DataContext>()
                .AddRepository<ResponseTest, DataContext>()
                .AddRepository<WebCheck, DataContext>();
        }
    }
}
