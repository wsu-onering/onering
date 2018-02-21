using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
            Database.Database db = new Database.Database(this.Configuration);
            InitializePortlets(db);

            if (!env.IsDevelopment()) {
                return;
            }
            Debug.Print("We've ensured that the database tables do exist.");
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
            Debug.Print(JsonConvert.SerializeObject(portlet, Formatting.Indented));
            Debug.Print(JsonConvert.SerializeObject(db.ListPortlets(), Formatting.Indented));
            Debug.Print(JsonConvert.SerializeObject(db.ListPortlets("Ex"), Formatting.Indented));

        }

        /// <summary>
        /// Uses reflection to find all classes implementing IPortlet, then initializes each Portlet
        /// in the provided database, if that portlet doesn't exist in the database already.
        /// </summary>
        /// <param name="db">an IOneRingDB, used to read portlet state and initialize new portlets.</param>
        private static void InitializePortlets(Database.IOneRingDB db) {
            List<Portlet> existing = db.ListPortlets();

            // PropertyInfo[] portletProps = typeof(Models.Interfaces.IPortlet).GetProperties();
            foreach (Type portletClass in GetTypesImplementingInterface<Models.Interfaces.IPortlet>()) {
                // Create an instance of the IPortlet
                Models.Interfaces.IPortlet portlet = Activator.CreateInstance(portletClass) as Models.Interfaces.IPortlet;
                // Get the name of the portlet through reflection
                PropertyInfo nameInf = portletClass.GetProperty("PortletName");
                string currentName = (string)nameInf.GetValue(portlet);

                // Check if a portlet of the given name already exists in the DB. If it does exist,
                // do not insert this portlet into the DB.
                bool weExist = false;
                foreach (Portlet p in db.ListPortlets(currentName)) {
                    if (p.Name == currentName) {
                        weExist = true;
                        Debug.WriteLine("We found a portlet with the same name as us: their name {0}, our name {1}", p.Name, currentName);
                    }
                }
                if (weExist) {
                    continue;
                }
                // This portlet doesn't exist in the DB, so let's insert it now.
                Portlet newP = new Portlet {
                    Name = currentName,
                    Description = (string)portletClass.GetProperty("PortletDescription").GetValue(portlet),
                    Path = (string)portletClass.GetProperty("PortletPath").GetValue(portlet),
                    Icon = (string)portletClass.GetProperty("PortletIconPath").GetValue(portlet)
                };
                Debug.WriteLine("The portlet {0} doesn't exist in the DB, creating it now.", currentName, "");
                db.CreatePortlet(newP);
            }
        }

        /// <summary>
        /// The intended use of this method is to find all types implementing IPortlet, then call the
        /// various static properties of those types, providing metadata about each Portlet.
        /// </summary>
        /// <returns>Returns an IEnumerable of all the types in the entry assembly which implement the interface T.</returns>
        private static IEnumerable<Type> GetTypesImplementingInterface<T>()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            foreach (TypeInfo ti in assembly.DefinedTypes)
            {
                if (ti.ImplementedInterfaces.Contains(typeof(T)))
                {
                    // Debug.WriteLine("    !!!! Found type {0} implementing type {1} !!!!", ti.FullName, typeof(T).FullName);
                    yield return ti.AsType();
                }
            }

            AssemblyName[] assemblies = assembly.GetReferencedAssemblies();
            foreach (AssemblyName assemblyName in assemblies)
            {
                assembly = Assembly.Load(assemblyName);
                foreach (TypeInfo ti in assembly.DefinedTypes)
                {
                    if (ti.ImplementedInterfaces.Contains(typeof(T)))
                    {
                        // Debug.WriteLine("    !!!! Found type {0} implementing type {1} !!!!", ti.FullName, typeof(T).FullName);
                        yield return ti.AsType();
                    }
                }
            }
        }
    }
}
