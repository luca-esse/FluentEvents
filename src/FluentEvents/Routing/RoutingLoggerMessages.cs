using System;
using FluentEvents.Pipelines;
using Microsoft.Extensions.Logging;

namespace FluentEvents.Routing
{
    internal static class RoutingLoggerMessages
    {
        private static readonly Func<ILogger, string, IDisposable> _beginEventRoutingScope = LoggerMessage.DefineScope<string>(
            "Routing event of type: {eventType}"
        );

        internal static IDisposable BeginEventRoutingScope(this ILogger logger, PipelineEvent pipelineEvent)
            => _beginEventRoutingScope(logger, pipelineEvent.EventType.Name);

        private static readonly Action<ILogger, Exception> _eventRoutedToPipeline = LoggerMessage.Define(
            LogLevel.Information,
            EventIds.EventRoutedToPipeline,
            "Routing event to pipeline"
        );

        internal static void EventRoutedToPipeline(this ILogger logger)
            => _eventRoutedToPipeline(logger, null);

        internal static class EventIds
        {
            public static EventId EventRoutedToPipeline { get; } = new EventId(1, nameof(EventRoutedToPipeline));
        }
    }
}
