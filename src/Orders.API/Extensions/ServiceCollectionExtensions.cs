using Asp.Versioning;
using Microsoft.OpenApi.Models;
using Orders.Infrastructure.Persistence;

namespace Orders.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        // API Versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // Swagger/OpenAPI
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Orders API",
                Version = "v1",
                Description = "A production-ready Orders API built with Clean Architecture",
                Contact = new OpenApiContact
                {
                    Name = "Christian",
                    Email = "christian@example.com"
                }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddHealthChecksServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddNpgSql(
                configuration.GetConnectionString("DefaultConnection")!,
                name: "postgresql",
                tags: ["db", "sql", "postgresql"])
            .AddRedis(
                configuration.GetConnectionString("Redis")!,
                name: "redis",
                tags: ["cache", "redis"]);

        return services;
    }
}
