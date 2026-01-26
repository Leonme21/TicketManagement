﻿using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Interfaces;
using TicketManagement.Infrastructure.Identity;
using TicketManagement.Infrastructure.Persistence;
using TicketManagement.Infrastructure.Persistence.Interceptors;
using TicketManagement.Infrastructure.Persistence.Repositories;
using TicketManagement.Infrastructure.Services;

namespace TicketManagement.Infrastructure;

/// <summary>
/// Registro de servicios de Infrastructure Layer
/// Se llama desde WebApi/Program.cs
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ==================== PERSISTENCE ====================

        // Interceptor para auditoría automática
        services.AddScoped<AuditableEntityInterceptor>();

        // DbContext con MySQL
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString));
        });

        // Initializer para migraciones y seeding
        services.AddScoped<ApplicationDbContextInitializer>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories (Explicit registration for Direct Injection in Queries)
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        // ==================== IDENTITY ====================

        // Servicios de autenticación
        services.AddSingleton<PasswordHasher>();
        services.AddSingleton<JwtTokenGenerator>();
        services.AddScoped<IIdentityService, IdentityService>();

        // Current User Service (necesita HttpContextAccessor)
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        // JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            var secret = jwtSettings?.Secret ?? throw new InvalidOperationException("JwtSettings:Secret is missing");
            var issuer = jwtSettings?.Issuer ?? throw new InvalidOperationException("JwtSettings:Issuer is missing");
            var audience = jwtSettings?.Audience ?? throw new InvalidOperationException("JwtSettings:Audience is missing");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // ==================== SERVICES ====================

        services.AddTransient<IDateTime, DateTimeService>();

        return services;
    }
}
