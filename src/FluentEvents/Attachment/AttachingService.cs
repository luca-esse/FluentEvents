using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentEvents.Infrastructure;
using FluentEvents.Model;
using FluentEvents.Pipelines;
using FluentEvents.Routing;

namespace FluentEvents.Attachment
{
    internal class AttachingService : IAttachingService
    {
        private readonly ISourceModelsService _sourceModelsService;
        private readonly IRoutingService _routingService;
        private readonly IEnumerable<IAttachingInterceptor> _attachingInterceptors;

        public AttachingService(
            ISourceModelsService sourceModelsService,
            IRoutingService routingService,
            IEnumerable<IAttachingInterceptor> attachingInterceptors
        )
        {
            _sourceModelsService = sourceModelsService;
            _routingService = routingService;
            _attachingInterceptors = attachingInterceptors;
        }

        public void Attach(object source, IEventsScope eventsScope)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (eventsScope == null) throw new ArgumentNullException(nameof(eventsScope));

            foreach (var attachingInterceptor in _attachingInterceptors)
                attachingInterceptor.OnAttaching(AttachInternal, source, eventsScope);

            AttachInternal(source, eventsScope);
        }

        private void AttachInternal(object source, IEventsScope eventsScope)
        {
            var sourceModel = _sourceModelsService.GetOrCreateSourceModel(source.GetType());

            foreach (var eventField in sourceModel.EventFields)
            {
                void HandlerAction(object e) =>
                    HandlerActionAsync(e).GetAwaiter().GetResult();

                Task HandlerActionAsync(object e) =>
                    _routingService.RouteEventAsync(new PipelineEvent(e), eventsScope);

                var eventHandler = eventField.IsAsync
                    ? sourceModel.CreateEventHandler<Func<object, Task>>(eventField, HandlerActionAsync)
                    : sourceModel.CreateEventHandler<Action<object>>(eventField, HandlerAction);

                eventField.EventInfo.AddEventHandler(source, eventHandler);
            }
        }
    }
}