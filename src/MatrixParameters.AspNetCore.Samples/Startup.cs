using MatrixParameters.AspNetCore.Constraints;
using MatrixParameters.AspNetCore.ModelBinders;
using MatrixParameters.AspNetCore.Swagger;
using Microsoft.OpenApi.Models;

namespace MatrixParameters.AspNetCore.Samples;

public class Startup
{
    public Startup(IConfiguration configuration) => Configuration = configuration;

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.ModelBinderProviders.Insert(0, new SegmentPrefixAttributeModelBinderProvider());
            options.ModelBinderProviders.Insert(1, new MatrixParameterAttributeModelBinderProvider());
        });

        services.AddRouting(options =>
        {
            options.ConstraintMap.Add("SegmentPrefix", typeof(SegmentPrefixConstraint));
        });

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "WebApplication1",
                Version = "v1"
            });
            c.ParameterFilter<MatrixParameterFilter>();
            c.DocumentFilter<MatrixDocumentFilter>();
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication1 v1"));

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}