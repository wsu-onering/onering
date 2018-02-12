using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using onering.Extensions;
using onering.Helpers;
using onering.Models;

namespace onering
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public const string ObjectIdentifierType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public const string TenantIdType = "http://schemas.microsoft.com/identity/claims/tenantid";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddAzureAd(options => Configuration.Bind("AzureAd", options))
            .AddCookie();
            services.AddMvc();
            // For right now, we use an in-memory cache for tokens and
            // subscriptions. In the future, we'll want to use some method of
            // persistent storage.
            services.AddMemoryCache();
            services.AddSession();

            // Add application services.
            services.AddSingleton<Database.IOneRingDB, Database.Database>();
            services.AddSingleton<IGraphAuthProvider, GraphAuthProvider>();
            services.AddTransient<IGraphSdkHelper, GraphSdkHelper>();
        }

        // This method gets called by the runtime. Use this method to configure
        // the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Configure and setup our database(s)
            this.ConfigureDatabase(env);

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            Debug.Print("Are we in development mode?: {0}", env.IsDevelopment());
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseSession();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Configures the various databses we're tasked with. If we're in Development mode, then we
        // populate those databases with some example values.
        private void ConfigureDatabase(IHostingEnvironment env) {
            Database.Database.EnsureTablesCreated(this.Configuration.GetSection("Databases")["OneRing"]);

            if (!env.IsDevelopment()) {
                return;
            }
            Debug.WriteLine("We've ensured that the database tables do exist.");
            Portlet portlet = new Portlet {
                Name = "ExamplePortlet",
                Description = "This is the example portlet.",
                Path = "/TodoPortlet/Index",
                Icon = "https://placeimg.com/150/150/tech",
                ConfigFields = new List<ConfigField>{
                    new ConfigField {
                        Name = "The options available:",
                        Description = "Indeed this is a field, with some options for things you can have.",
                        ConfigFieldOptions = new List<ConfigFieldOption> {
                            new ConfigFieldOption{ Value = "Red fish" },
                            new ConfigFieldOption{ Value = "Blue fish" },
                        }
                    },
                    new ConfigField {
                        Name = "Freeform input field:",
                        Description = "As a user, you can input whatever your heart desires into here."
                    }
                }
            };
            Database.Database db = new Database.Database(this.Configuration);

            if (!db.ListPortlets().Any()) {
                db.CreatePortlet(portlet);
                db.CreatePortlet(new Portlet {
                    Name = "ExtraCoolPortlet",
                    Description = "An extra cool portlet.",
                    Path = "",
                    Icon = "https://placeimg.com/150/150/tech",
                });
                db.CreatePortlet(new Portlet {
                    Name = "NotAsCoolPortlet",
                    Description = "A portlet that is not as cool.",
                    Path = "",
                    Icon = "https://placeimg.com/150/150/tech",
                });
            }
            Debug.WriteLine(JsonConvert.SerializeObject(portlet, Formatting.Indented));
            Debug.WriteLine(JsonConvert.SerializeObject(db.ListPortlets(), Formatting.Indented));
            Debug.WriteLine(JsonConvert.SerializeObject(db.ListPortlets("Ex"), Formatting.Indented));

        }
    }
}
