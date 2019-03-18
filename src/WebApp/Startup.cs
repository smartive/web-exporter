using DataAccess;
using DataAccess.Repositories;
using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Prometheus;
using Prometheus.Advanced;
using React.AspNet;
using WebApp.Metrics;

namespace WebApp
{
    /// <summary>
    /// Startup.
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Create startup environment.
        /// </summary>
        /// <param name="env">HostingEnv</param>
        public Startup(IHostingEnvironment env)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                .Build();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">DI Container</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddReact()
                .AddJsEngineSwitcher(options => options.DefaultEngineName = ChakraCoreJsEngine.EngineName)
                .AddChakraCore();

            services.AddNodeServices();

            services
                .AddRouting(options => options.LowercaseUrls = true)
                .AddResponseCompression()
                .AddMvc()
                .AddJsonOptions(
                    options =>
                    {
                        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    });

            services
                .AddLogging(
                    builder => builder
                        .AddConsole()
                        .AddConfiguration(_configuration));

            services
                .AddDbContext<DataContext>(
                    options => options.UseSqlite("Data Source=./Data/web-exporter.sqlite"))
                .AddScoped<WebCheckExecutor>()
                .AddSingleton<PrometheusCollector>()
                .AddSingleton<WebCheckResults>()
                .AddDataAccess();

            services.AddHealthChecks();

            services
                .AddHostedService<WebCheckHostedService>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="env">Environment</param>
        /// <param name="context">DataContext</param>
        /// <param name="collector">Collector</param>
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            DataContext context,
            PrometheusCollector collector)
        {
            app.UseReact(
                config =>
                {
                    config
                        .SetReuseJavaScriptEngines(true)
                        .SetLoadBabel(false)
                        .SetLoadReact(false)
                        .SetJsonSerializerSettings(
                            new JsonSerializerSettings
                            {
                                ContractResolver = new CamelCasePropertyNamesContractResolver()
                            })
                        .AddScriptWithoutTransform("~/js/app.js");
                });

            if (env.IsDevelopment())
            {
                app
                    .UseDeveloperExceptionPage()
                    .UseDatabaseErrorPage()
                    .UseStaticFiles(
                        new StaticFileOptions
                        {
                            OnPrepareResponse = ctx =>
                            {
                                ctx.Context.Response.Headers[HeaderNames.CacheControl] = "no-cache";
                            }
                        });
            }
            else
            {
                app.UseForwardedHeaders(
                    new ForwardedHeadersOptions
                    {
                        ForwardedHeaders = ForwardedHeaders.XForwardedProto
                    });

                app.UseStaticFiles(
                    new StaticFileOptions
                    {
                        OnPrepareResponse = ctx =>
                        {
                            ctx.Context.Response.Headers[HeaderNames.Pragma] = "public";
                            ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                                "public, max-age=31536000, immutable";
                        }
                    });
            }

            app.UseHealthChecks("/health");
            app.UseMvc();

            DefaultCollectorRegistry.Instance.Clear();
            DefaultCollectorRegistry.Instance.GetOrAdd(collector);
            app.UseMetricServer();

            context.Database.Migrate();
        }
    }
}
