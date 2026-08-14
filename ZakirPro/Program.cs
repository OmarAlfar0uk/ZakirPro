using Serilog;
using ZakirPro.Common.Extensions;
using ZakirPro.Common.Middleware;
using ZakirPro.Data.Seeders;

namespace ZakirPro;

public class Program
{
    public static async Task Main(string[] args)
    {
        // ── Serilog bootstrap logger (captures startup errors) ────────────────
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting Zaker Pro API...");

            var builder = WebApplication.CreateBuilder(args);

            // ── Serilog (replaces default .NET logging) ───────────────────────
            builder.Host.UseSerilog((ctx, services, cfg) =>
                cfg.ReadFrom.Configuration(ctx.Configuration)
                   .ReadFrom.Services(services)
                   .Enrich.FromLogContext()
                   .WriteTo.Console()
                   .WriteTo.File("logs/zakirpro-.log",
                       rollingInterval: RollingInterval.Day,
                       retainedFileCountLimit: 30));

            // ── Application services ──────────────────────────────────────────
            builder.Services.AddApplicationServices(builder.Configuration);
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddAuthorizationPolicies();
            builder.Services.AddSwaggerWithJwt();

            // ── CORS (allow all for development) ──────────────────────────────
            builder.Services.AddCors(opts =>
                opts.AddDefaultPolicy(p =>
                    p.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader()));

            var app = builder.Build();

            // ── Seed SuperAdmin ───────────────────────────────────────────────
            await SuperAdminSeeder.SeedAsync(app.Services);

            // ── Middleware pipeline ───────────────────────────────────────────
            app.UseMiddleware<GlobalExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zaker Pro API v1");
                    c.RoutePrefix = string.Empty; // serve Swagger at root
                });
            }

            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            // ── Register all IEndpointDefinition slices ───────────────────────
            app.MapEndpointDefinitions();

            app.Run();
        }
        catch (Exception ex) when (ex is not HostAbortedException)
        {
            Log.Fatal(ex, "Zaker Pro API terminated unexpectedly.");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
