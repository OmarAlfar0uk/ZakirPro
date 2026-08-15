using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Behaviors;
using ZakirPro.Common.Infrastructure;
using ZakirPro.Data;

namespace ZakirPro.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        // ── Database ──────────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        // ── MediatR + Validation Pipeline ─────────────────────────────────────
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Program>();
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // ── FluentValidation — scan entire assembly ───────────────────────────
        services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);

        // ── Infrastructure services ───────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITokenService, JwtService>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<IEmailService, MailKitEmailService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ── Background services ───────────────────────────────────────────────
        services.AddHostedService<ZakirPro.Common.BackgroundServices.ExamAutoSubmitService>();

        // ── Caching ───────────────────────────────────────────────────────────
        services.AddMemoryCache();

        // ── HTTP context ──────────────────────────────────────────────────────
        services.AddHttpContextAccessor();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidIssuer              = config["JwtSettings:Issuer"],
                    ValidateAudience         = true,
                    ValidAudience            = config["JwtSettings:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(
                                                  Encoding.UTF8.GetBytes(config["JwtSettings:Key"]!)),
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero
                };
            });

        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(opts =>
        {
            opts.AddPolicy("SuperAdminOnly",
                p => p.RequireRole("SuperAdmin"));

            opts.AddPolicy("AdminOrAbove",
                p => p.RequireRole("Admin", "SuperAdmin"));

            opts.AddPolicy("TeacherOrAbove",
                p => p.RequireRole("Teacher", "Admin", "SuperAdmin"));

            opts.AddPolicy("AssistantOrAbove",
                p => p.RequireRole("Assistant", "Teacher", "Admin", "SuperAdmin"));

            opts.AddPolicy("StudentOnly",
                p => p.RequireRole("Student"));

            opts.AddPolicy("AuthenticatedUser",
                p => p.RequireAuthenticatedUser());
        });

        return services;
    }

    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title   = "Zaker Pro API",
                Version = "v1",
                Description = "SaaS educational platform for independent teachers."
            });

            // JWT bearer button in Swagger UI
            var securityScheme = new OpenApiSecurityScheme
            {
                Name         = "Authorization",
                Type         = SecuritySchemeType.Http,
                Scheme       = "bearer",
                BearerFormat = "JWT",
                In           = ParameterLocation.Header,
                Description  = "Enter your JWT token (without 'Bearer ' prefix)."
            };
            c.AddSecurityDefinition("Bearer", securityScheme);
            c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", doc, null),
                    new List<string>()
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Scans the assembly for all IEndpointDefinition implementations and registers them.
    /// </summary>
    public static WebApplication MapEndpointDefinitions(this WebApplication app)
    {
        var definitions = typeof(Program).Assembly
            .GetTypes()
            .Where(t => typeof(IEndpointDefinition).IsAssignableFrom(t)
                        && t is { IsInterface: false, IsAbstract: false })
            .Select(Activator.CreateInstance)
            .Cast<IEndpointDefinition>();

        foreach (var def in definitions)
            def.DefineEndpoints(app);

        return app;
    }
}
