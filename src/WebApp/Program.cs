using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace WebApp
{
    /// <summary>
    /// WebApp application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main method of the application.
        /// </summary>
        /// <param name="args">Arguments</param>
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Application startup.
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <returns>WebApp</returns>
        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
