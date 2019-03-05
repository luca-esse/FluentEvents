﻿namespace FluentEvents.Azure.ServiceBus.Receiving
{
    /// <inheritdoc />
    /// <summary>
    ///     An exception thrown when the <see cref="TopicEventReceiverConfig.ManagementConnectionString" /> property is null.
    /// </summary>
    public class ManagementConnectionStringIsNullException : FluentEventsServiceBusException
    {
    }
}