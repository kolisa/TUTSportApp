using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using TUTSportApp.Application;
using TUTSportApp.Infrastructure;
using System.Globalization;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Full Serilog config from appsettings
    builder.Host.UseSerilog((context, services, cfg) =>
        cfg.ReadFrom.Configuration(context.Configuration)
           .ReadFrom.Services(services));

    // Add Application Insights
    builder.Services.AddApplicationInsightsTelemetry();

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "Sport Management API", Version = "v1", Description = "Sport Management API" });
        var xml = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    });

    TUTSportApp.Application.ApplicationServiceRegistration.AddApplication(builder.Services);
    TUTSportApp.Infrastructure.InfrastructureServiceRegistration.AddInfrastructure(builder.Services, builder.Configuration);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sport Management API v1");
            c.RoutePrefix = "swagger"; // UI lives at /swagger
        });
    }

    app.UseHttpsRedirection();

    app.UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    await app.RunAsync().ConfigureAwait(false);
}
finally
{
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}


// This must be the last thing in the file:
namespace TUTSportApp.Api
{
    public partial class Program { }
}

