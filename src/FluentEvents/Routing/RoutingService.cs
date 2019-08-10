using System.Threading.Tasks;
using FluentEvents.Infrastructure;
using FluentEvents.Pipelines;
using Microsoft.Extensions.Logging;

namespace FluentEvents.Routing
{
    internal class RoutingService : IRoutingService
    {
        private readonly ILogger<RoutingService> _logger;
        private readonly IPipelinesService _pipelinesService;

        public RoutingService(
            ILogger<RoutingService> logger,
            IPipelinesService pipelinesService
        )
        {
            _logger = logger;
            _pipelinesService = pipelinesService;
        }

        public async Task RouteEventAsync(PipelineEvent pipelineEvent, IEventsScope eventsScope)
        {
            using (_logger.BeginEventRoutingScope(pipelineEvent))
            {
                var pipelines = _pipelinesService.GetPipelines(pipelineEvent.EventType);

                foreach (var pipeline in pipelines)
                {
                    _logger.EventRoutedToPipeline();

                    await pipeline.ProcessEventAsync(pipelineEvent, eventsScope).ConfigureAwait(false);
                }
            }
        }
    }
}
