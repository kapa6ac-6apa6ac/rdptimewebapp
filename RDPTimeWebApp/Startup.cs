using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RDPTimeWebApp.DbContexts;
using VueCliMiddleware;

namespace RDPTimeWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddDbContext<OrionContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("OrionConnection"));
            });

            services.AddDbContext<SigurMainContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("SigurMainConnection"));
            });

            services.AddDbContext<SigurLogsContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("SigurLogsConnection"));
            });
            
            services
                .AddHttpClient("timemanic", c => { })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new System.Net.Http.HttpClientHandler
                    {
                        UseDefaultCredentials = true,
                        Credentials = new System.Net.NetworkCredential("20eia", "Fgh7Qrst")
                    };
                });

            services.AddScoped<Services.ScudLogGrabber>();
            services.AddSingleton<Services.VectorService>();
            services.AddScoped<Services.TimeManicService>();
            services.AddScoped<Services.CalendarService>();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = Configuration["Auth:Authority"];
                    options.Audience = Configuration["Auth:Audience"];
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
                    };
                });

            services.AddSpaStaticFiles(opt => opt.RootPath = "client-app-new/dist");

            services.AddControllers();

            services.AddSwaggerDocument();

            //services.AddSpaStaticFiles(configuration =>
            //{
            //    configuration.RootPath = "wwwroot";
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();

            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseOpenApi();
                app.UseSwaggerUi3();
                app.UseReDoc(config =>
                {
                    config.Path = "/redoc";
                    config.DocumentPath = "/swagger/v1/swagger.json";
                });
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

            //    endpoints.MapToVueCliProxy(
            //        "{*path}",
            //        new Microsoft.AspNetCore.SpaServices.SpaOptions { SourcePath = "client-app-new" },
            //        npmScript: (System.Diagnostics.Debugger.IsAttached) ? "serve" : null,
            //        regex: "(Compiled successfully|Compiled with (.*))",
            //        forceKill: true
            //        );
            });

            app.UseSpa(spa =>
            {
#if DEBUG
                if (env.IsDevelopment())
                    spa.Options.SourcePath = "client-app-new\\dist";
                else
                    spa.Options.SourcePath = "dist";

                if (env.IsDevelopment())
                {
                //    spa.UseVueCli(npmScript: "serve");
                }
#endif
            });
        }
    }
}
