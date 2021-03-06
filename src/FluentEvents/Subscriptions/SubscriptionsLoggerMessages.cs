﻿using System;
using FluentEvents.Pipelines;
using Microsoft.Extensions.Logging;

namespace FluentEvents.Subscriptions
{
    internal static class SubscriptionsLoggerMessages
    {
        private static readonly Action<ILogger, Exception> _eventHandlerThrew = LoggerMessage.Define(
            LogLevel.Error,
            EventIds.EventHandlerThrew,
            "An event handler threw an exception"
        );

        internal static void EventHandlerThrew(this ILogger logger, Exception exception)
            => _eventHandlerThrew(logger, exception);

        private static readonly Action<ILogger, string, Exception> _publishingEvent = LoggerMessage.Define<string>(
            LogLevel.Information,
            EventIds.PublishingEvent,
            "Publishing event of type: {eventType}"
        );

        internal static void PublishingEvent(this ILogger logger, PipelineEvent pipelineEvent)
            => _publishingEvent(
                logger,
                pipelineEvent.EventType.Name,
                null
            );

        internal static class EventIds
        {
            public static EventId EventHandlerThrew { get; } = new EventId(1, nameof(EventHandlerThrew));
            public static EventId PublishingEvent { get; } = new EventId(2, nameof(PublishingEvent));
        }
    }
}
