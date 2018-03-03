using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace TomKerkhove.Samples.KeyVault.API
{
    public class Startup
    {
        private const string OpenApiTitle = "Azure Key Vault Samples";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(swaggerUiOptions =>
            {
                swaggerUiOptions.SwaggerEndpoint("/swagger/v1/swagger.json", OpenApiTitle);
                swaggerUiOptions.DisplayOperationId();
                swaggerUiOptions.DocumentTitle = OpenApiTitle;
                swaggerUiOptions.DocExpansion(DocExpansion.None);
                swaggerUiOptions.DisplayRequestDuration();
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var openApiInformation = new Info
            {
                Contact = new Contact
                {
                    Name = "Tom Kerkhove"
                },
                Title = $"{OpenApiTitle} v1",
                Description = "Collection of samples how you can use Azure Key Vault",
                Version = "v1"
            };
            
            services.AddSwaggerGen(swaggerGenerationOptions =>
            {
                swaggerGenerationOptions.SwaggerDoc("v1", openApiInformation);
                swaggerGenerationOptions.DescribeAllEnumsAsStrings();
            });
        }
    }
}