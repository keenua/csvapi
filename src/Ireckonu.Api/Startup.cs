using Ireckonu.Api.Converters;
using Ireckonu.Api.Helpers;
using Ireckonu.BusinessLogic;
using Ireckonu.BusinessLogic.Converters;
using Ireckonu.BusinessLogic.Services;
using Ireckonu.Data;
using Ireckonu.Data.Json;
using Ireckonu.Data.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ireckonu
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
            services.AddControllers();
            services.AddMvcCore().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
            });

            services.AddScoped<IUploadService, UploadService>();
            services.AddSingleton<IDtoConverter, DtoConverter>();
            services.AddScoped<ICsvProcessor, CsvProcessor>();
            services.AddSingleton<IModelConverter, ModelConverter>();

            services.AddSingleton(Configuration.GetSection("JsonDb").Get<JsonDbSettings>());
            services.AddSingleton(Configuration.GetSection("MongoDb").Get<MongoSettings>());
            services.AddTransient<JsonDbContext>();
            services.AddTransient<MongoDbContext>();

            services.AddScoped<IDbContext>(sp =>
            {
                var mongo = sp.GetRequiredService<MongoDbContext>();
                var json = sp.GetRequiredService<JsonDbContext>();
                var contexts = new IDbContext[] { mongo, json };

                var logger = sp.GetService<ILogger<AggregateDbContext>>();
                var context = new AggregateDbContext(contexts, logger);
                return context;
            });

            services.AddSwaggerGen(o =>
            {
                o.OperationFilter<FileUploadHelper>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ireckonu API");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
