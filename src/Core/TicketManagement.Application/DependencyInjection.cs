﻿using System.Reflection;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TicketManagement.Application.Common.Behaviors;
using TicketManagement.Application.Common.Mappings;

namespace TicketManagement.Application;

/// <summary>
/// Registro de servicios de Application Layer
/// Se llama desde WebApi/Program.cs
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // AutoMapper: Usar registro estándar para permitir DI en perfiles/resolvers
        // AutoMapper: Manual registration to avoid build ambiguity with extension methods
        var mapperConfig = new MapperConfiguration(cfg => 
        {
            cfg.AddMaps(assembly);
        });
        var mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);

        // FluentValidation (escanea todos los AbstractValidator<>)
        services.AddValidatorsFromAssembly(assembly);

        // MediatR (escanea todos los IRequestHandler<,>)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Pipeline Behaviors (se ejecutan en orden)
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        return services;
    }
}