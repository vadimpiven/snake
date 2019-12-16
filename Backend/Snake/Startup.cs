using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSwag;
using Snake.Core;

namespace Snake
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
            services.AddSingleton<IGame, Game>();

            services.AddCors();

            services.AddControllers();

            services.AddSwaggerDocument(config =>
            {
                config.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "Snake Game";
                    document.Info.Description = "Kaspersky SafeBoard C# project";
                    document.Info.Contact = new OpenApiContact
                    {
                        Name = "Vadim Piven",
                        Url = "https://piven.tech/hi/"
                    };
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                logger.LogInformation("Starting in developer mode");
                app.UseDeveloperExceptionPage();

                logger.LogInformation("Setting up swagger endpoint");
                app.UseOpenApi(settings =>
                {
                    settings.Path = "/api/swagger/" + "v1" + "/swagger.json";
                    settings.PostProcess = (document, request) =>
                    {
                        document.Host = request.Headers["X-Forwarded-Host"].FirstOrDefault();
                    };
                });
                app.UseSwaggerUi3(options =>
                {
                    options.Path = "/api/swagger";
                    options.DocumentPath = "/api/swagger/" + "v1" + "/swagger.json";
                });

                app.UseCors(builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            }
            else
            {
                // TODO: configure CORS for production
                app.UseCors();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            logger.LogInformation("Setting up controller endpoints");
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}