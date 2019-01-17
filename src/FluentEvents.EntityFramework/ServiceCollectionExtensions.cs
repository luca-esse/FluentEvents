﻿using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using FluentEvents.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace FluentEvents.EntityFramework
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDbContextWithEntityEventsAttachedTo<TDbContext, TEventsContext>(this IServiceCollection services)
            where TDbContext : DbContext
            where TEventsContext : EventsContext
        {
            var proxyDbContextType = FluentEventsProxyBuilder.CreateClassProxyType(typeof(TDbContext));

            services.Add(new ServiceDescriptor(proxyDbContextType, proxyDbContextType, ServiceLifetime.Scoped));
            services.Add(new ServiceDescriptor(typeof(TDbContext), x =>
            {
                var dbContext = (TDbContext)x.GetRequiredService(proxyDbContextType);
                var eventsContext = x.GetRequiredService<TEventsContext>();
                var eventsScope = x.GetRequiredService<EventsScope>();
                ((IObjectContextAdapter)dbContext).ObjectContext.ObjectMaterialized += delegate (object sender, ObjectMaterializedEventArgs e)
                {
                    eventsContext.Attach(e.Entity, eventsScope);
                };
                return dbContext;
            }, ServiceLifetime.Scoped));

            return services;
        }
    }
}