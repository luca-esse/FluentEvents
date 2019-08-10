using System;
using FluentEvents.Configuration;
using FluentEvents.Infrastructure;
using FluentEvents.Queues;
using Microsoft.Extensions.DependencyInjection;

namespace FluentEvents.Pipelines.Queues
{
    /// <summary>
    ///     Extension methods for adding a pipeline module that queues the events.
    /// </summary>
    public static class EventPipelineConfigurationExtensions
    {
        /// <summary>
        ///     Adds module to the current pipeline that queues the event in a queue
        ///     and pauses the execution of the current pipeline until the event is dequeued.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="eventPipelineConfiguration">
        ///     The <see cref="EventPipelineConfiguration{TEvent}"/> for the pipeline being configured.
        /// </param>
        /// <param name="queueName">The name of the queue.</param>
        /// <returns>
        ///     The same <see cref="EventPipelineConfiguration{TEvent}"/> instance so that multiple calls can be chained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="eventPipelineConfiguration"/> and/or <paramref name="queueName"/> are <see langword="null"/>.
        /// </exception>
        public static EventPipelineConfiguration<TEvent> ThenIsQueuedTo<TEvent>(
            this EventPipelineConfiguration<TEvent> eventPipelineConfiguration,
            string queueName
        )
            where TEvent : class
        {
            if (eventPipelineConfiguration == null) throw new ArgumentNullException(nameof(eventPipelineConfiguration));
            if (queueName == null) throw new ArgumentNullException(nameof(queueName));

            var eventsQueueNamesService = eventPipelineConfiguration
                .Get<IServiceProvider>()
                .GetRequiredService<IEventsQueueNamesService>();

            eventsQueueNamesService.RegisterQueueNameIfNotExists(queueName);

            eventPipelineConfiguration
                .Get<IPipeline>()
                .AddModule<EnqueuePipelineModule, EnqueuePipelineModuleConfig>(
                    new EnqueuePipelineModuleConfig
                    {
                        QueueName = queueName
                    }
                );

            return eventPipelineConfiguration;
        }
    }
}
