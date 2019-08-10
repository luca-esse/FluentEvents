using System;
using FluentEvents.Infrastructure;

namespace FluentEvents.Pipelines
{
    /// <summary>
    ///     Represents a pipeline processing context. 
    /// </summary>
    public class PipelineContext
    {
        /// <summary>
        ///     The event being processed.
        /// </summary>
        public PipelineEvent PipelineEvent { get; set; }

        /// <summary>
        ///     The current events scope.
        /// </summary>
        public IEventsScope EventsScope { get; }

        /// <summary>
        ///     Creates a new instance of a <see cref="PipelineContext"/>.
        /// </summary>
        /// <param name="pipelineEvent">The event being processed.</param>
        /// <param name="eventsScope">The current events scope.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="pipelineEvent"/> and/or <paramref name="eventsScope"/> are <see langword="null"/>.
        /// </exception>
        public PipelineContext(PipelineEvent pipelineEvent, IEventsScope eventsScope)
        {
            PipelineEvent = pipelineEvent ?? throw new ArgumentNullException(nameof(pipelineEvent));
            EventsScope = eventsScope ?? throw new ArgumentNullException(nameof(eventsScope));
        }
    }
}