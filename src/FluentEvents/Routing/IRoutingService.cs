using System.Threading.Tasks;
using FluentEvents.Infrastructure;
using FluentEvents.Pipelines;

namespace FluentEvents.Routing
{
    internal interface IRoutingService
    {
        Task RouteEventAsync(PipelineEvent pipelineEvent, IEventsScope eventsScope);
    }
}