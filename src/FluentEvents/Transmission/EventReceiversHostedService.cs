﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace FluentEvents.Transmission
{
    internal class EventReceiversHostedService : IHostedService
    {
        private readonly IEventReceiversService _eventReceiversService;

        public EventReceiversHostedService(IEventReceiversService eventReceiversService)
        {
            _eventReceiversService = eventReceiversService;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _eventReceiversService.StartReceiversAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _eventReceiversService.StopReceiversAsync(cancellationToken);
        }
    }
}