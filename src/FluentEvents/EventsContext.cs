using System;
using System.Threading;
using System.Threading.Tasks;
using FluentEvents.Attachment;
using FluentEvents.Configuration;
using FluentEvents.Infrastructure;
using FluentEvents.Queues;
using FluentEvents.ServiceProviders;
using FluentEvents.Transmission;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FluentEvents
{
    /// <summary>
    ///     <para>
    ///         The <see cref="EventsContext"/> provides the API surface to configure the event pipelines and the event subscriptions.
    ///     </para>
    ///     <para>
    ///         An <see cref="EventsContext"/> should be treated as a singleton.
    ///     </para>
    /// </summary>
    public abstract class EventsContext
    {
        private readonly Lazy<InternalEventsContext> _internalEventsContext;

        /// <summary>
        ///     Creates a new <see cref="EventsContext"/>
        /// </summary>
        /// <param name="options">The options for this context.</param>
        /// <param name="rootAppServiceProvider">The application root service provider.</param>
        protected EventsContext(
            EventsContextOptions options,
            IRootAppServiceProvider rootAppServiceProvider
        )
        {
            _internalEventsContext = new Lazy<InternalEventsContext>(() => new InternalEventsContext(
                options,
                OnConfiguring,
                OnBuildingPipelines,
                OnBuildingSubscriptions,
                rootAppServiceProvider
            ), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private InternalEventsContext GetCurrentContext() => _internalEventsContext.Value;

        private IServiceProvider GetCurrentInternalServiceProvider() => GetCurrentContext().InternalServiceProvider;

        /// <summary>
        ///     Override this method to alter the options supplied in the constructor.
        ///     The resulting configuration may be cached and re-used during the entire lifespan of the context.
        /// </summary> 
        /// <remarks>The default implementation of this method does nothing.</remarks>
        /// <param name="options">The options of the <see cref="EventsContext"/>.</param>
        protected virtual void OnConfiguring(EventsContextOptions options) { }

        /// <summary>
        ///     Override this method to configure the event subscriptions.
        ///     The resulting configuration may be cached and re-used during the entire lifespan of the context.
        /// </summary>
        /// <remarks>The default implementation of this method does nothing.</remarks>
        /// <param name="subscriptionsBuilder">The builder that defines the model for the context being created.</param>
        protected virtual void OnBuildingSubscriptions(ISubscriptionsBuilder subscriptionsBuilder) { }

        /// <summary>
        ///     Override this method to configure the pipelines needed for handling the events.
        ///     The resulting configuration may be cached and re-used during the entire lifespan of the context.
        /// </summary>
        /// <remarks>The default implementation of this method does nothing.</remarks>
        /// <param name="pipelinesBuilder">The builder that defines the model for the context being created.</param>
        protected virtual void OnBuildingPipelines(IPipelinesBuilder pipelinesBuilder) { }

        /// <summary>
        ///     <para>
        ///          Subscribes the <see cref="EventsContext"/> to every source's event field having a delegate with
        ///          a single parameter (of any type) and a return type of <see cref="Task"/> or <see langword="void"/>.
        ///     </para>
        ///     <para>
        ///         Examples of valid delegates:
        ///         <see cref="Func{TEvent, TTask}">Func&lt;<see cref="object"/>, <see cref="Task"/>&gt;</see> or
        ///         <see cref="Action{TEvent}" /> or
        ///         <see cref="AsyncEventPublisher{TEvent}" /> or
        ///         <see cref="EventPublisher{TEvent}" />
        ///     </para>
        ///     <para>
        ///         This subscription allows the <see cref="EventsContext"/> to forward the events
        ///         to the corresponding pipelines.
        ///     </para>
        /// </summary>
        /// <param name="source">The event source.</param>
        /// <param name="eventsScope">The scope of the events published from this source.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source"/> and/or <paramref name="eventsScope"/> are <see langword="null"/>.
        /// </exception>
        public virtual void WatchSourceEvents(object source, EventsScope eventsScope)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (eventsScope == null) throw new ArgumentNullException(nameof(eventsScope));

            GetCurrentInternalServiceProvider()
                .GetRequiredService<IAttachingService>()
                .Attach(source, eventsScope);
        }

        /// <summary>
        ///     Resumes the processing of the events that have been queued.
        /// </summary>
        /// <param name="eventsScope">The scope where the queued events were published.</param>
        /// <param name="queueName">
        ///     The name of the queue.
        ///     If <see langword="null"/> all the events will be processed.
        /// </param>
        /// <exception cref="EventsQueueNotFoundException">
        ///     No queues were found with the supplied <paramref name="queueName"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="eventsScope"/> is <see langword="null"/>.
        /// </exception>
        public virtual Task ProcessQueuedEventsAsync(EventsScope eventsScope, string queueName = null)
        {
            if (eventsScope == null) throw new ArgumentNullException(nameof(eventsScope));

            return GetCurrentInternalServiceProvider()
                .GetRequiredService<IEventsQueuesService>()
                .ProcessQueuedEventsAsync(eventsScope, queueName);
        }

        /// <summary>
        ///     Discards all the events of a queue.
        /// </summary>
        /// <param name="eventsScope">The scope where the queued events were published.</param>
        /// <param name="queueName">
        ///     The name of the queue.
        ///     If <see langword="null"/> all the events will be discarded.
        /// </param>
        /// <exception cref="EventsQueueNotFoundException">
        ///     No queues were found with the supplied <paramref name="queueName"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="eventsScope"/> is <see langword="null"/>.
        /// </exception>
        public virtual void DiscardQueuedEvents(EventsScope eventsScope, string queueName = null)
        {
            if (eventsScope == null) throw new ArgumentNullException(nameof(eventsScope));

            GetCurrentInternalServiceProvider()
                .GetRequiredService<IEventsQueuesService>()
                .DiscardQueuedEvents(eventsScope, queueName);
        }

        /// <summary>
        ///     Returns an <see cref="IHostedService"/> that can start or stop the configured
        ///     event receivers.
        /// </summary>
        public IHostedService GetEventReceiversHostedService()
            => GetCurrentInternalServiceProvider()
                .GetRequiredService<EventReceiversHostedService>();
    }
}
