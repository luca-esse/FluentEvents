using System;
using System.Collections.Generic;
using FluentEvents.Attachment;
using FluentEvents.Configuration;
using FluentEvents.Model;
using FluentEvents.Pipelines;
using FluentEvents.Pipelines.Filters;
using FluentEvents.Pipelines.Projections;
using FluentEvents.Pipelines.Publication;
using FluentEvents.Pipelines.Queues;
using FluentEvents.Plugins;
using FluentEvents.Publication;
using FluentEvents.Queues;
using FluentEvents.Routing;
using FluentEvents.ServiceProviders;
using FluentEvents.Subscriptions;
using FluentEvents.Transmission;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace FluentEvents.Infrastructure
{
    internal class InternalServiceCollection : IInternalServiceCollection
    {
        public static readonly Type[] ServicesToIgnoreWhenAttaching = {typeof(ILoggerFactory)};

        private readonly IRootAppServiceProvider _rootAppServiceProvider;

        public InternalServiceCollection(IRootAppServiceProvider rootAppServiceProvider)
        {
            _rootAppServiceProvider = rootAppServiceProvider;
        }

        public ServiceProvider BuildServiceProvider(IEventsContextOptions options)
        {
            Func<IServiceProvider, object> GetLoggerFactory() =>
                x => _rootAppServiceProvider.GetService<ILoggerFactory>() ??
                     new LoggerFactory(x.GetService<IEnumerable<ILoggerProvider>>());

            var services = new ServiceCollection();
            services.AddLogging();
            services.Replace(new ServiceDescriptor(
                typeof(ILoggerFactory),
                GetLoggerFactory(),
                ServiceLifetime.Singleton
            ));

            services.AddSingleton(_rootAppServiceProvider);
            services.AddSingleton<PipelinesBuilder>();
            services.AddSingleton<SubscriptionsBuilder>();
            services.AddSingleton<IEventsQueuesService, EventsQueuesService>();
            services.AddSingleton<IScopedSubscriptionsService, ScopedSubscriptionsService>();
            services.AddSingleton<IRoutingService, RoutingService>();
            services.AddSingleton<IEventsQueueNamesService, EventsQueueNamesService>();
            services.AddSingleton<IEventsSerializationService, JsonEventsSerializationService>();
            services.AddSingleton<IGlobalSubscriptionsService, GlobalSubscriptionsService>();
            services.AddSingleton<ISourceModelsService, SourceModelsService>();
            services.AddSingleton<IPublishingService, PublishingService>();
            services.AddSingleton<IEventReceiversService, EventReceiversService>();
            services.AddSingleton<IAttachingService, AttachingService>();
            services.AddSingleton<ISubscriptionsMatchingService, SubscriptionsMatchingService>();
            services.AddSingleton<IPipelinesService, PipelinesService>();
            services.AddSingleton<EnqueuePipelineModule>();
            services.AddSingleton<FilterPipelineModule>();
            services.AddSingleton<GlobalPublishPipelineModule>();
            services.AddSingleton<ScopedPublishPipelineModule>();
            services.AddSingleton<ProjectionPipelineModule>();
            services.AddSingleton<EventReceiversHostedService>();

            foreach (var plugin in options.Plugins)
                plugin.ApplyServices(services);

            return services.BuildServiceProvider();
        }
    }
}
