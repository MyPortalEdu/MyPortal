using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MyPortal.Logic.Extensions;
using MyPortalWeb.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MyPortalWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMyPortal(builder => { builder.FromConfig(Configuration); });

            services.AddControllers();
            services.AddCors();

            services.AddIdentityServices(Configuration);

#if DEBUG
            services.AddSwaggerGen(c =>
            {
                c.CustomOperationIds(e => e.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null);

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "1.0.0",
                    Title = "MyPortal",
                    Description = "MyPortal API"
                });

                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });
#endif
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
#if DEBUG
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyPortal API v1");
                c.RoutePrefix = "swagger";
            });
#endif
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseMyPortal();
            //app.UseDefaultFiles();
            //app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapFallbackToController("Index", "Fallback");
            });
        }
    }
}